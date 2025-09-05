using AppSchoolMaui.Models;
using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppSchoolMaui.ViewModels
{
    public sealed class PublicVm : BaseViewModel
    {
        private readonly ApiService _api;
        public PublicVm(ApiService api)
        {
            _api = api;
            ShowSubjectsCommand = new Command(() => { IsSubjectsMode = true; IsCoursesMode = false; });
            ShowCoursesCommand = new Command(() => { IsCoursesMode = true; IsSubjectsMode = false; });
        }

        // ---- modo de visualização
        private bool _isSubjectsMode = true;
        public bool IsSubjectsMode { get => _isSubjectsMode; set => Set(ref _isSubjectsMode, value); }

        private bool _isCoursesMode;
        public bool IsCoursesMode { get => _isCoursesMode; set => Set(ref _isCoursesMode, value); }

        public ICommand ShowSubjectsCommand { get; }
        public ICommand ShowCoursesCommand { get; }

        // ---- dados
        public List<PublicSubjectItem> Subjects { get => _subjects; set => Set(ref _subjects, value); }
        private List<PublicSubjectItem> _subjects = new();

        public List<PublicCourseItem> Courses { get => _courses; set => Set(ref _courses, value); }
        private List<PublicCourseItem> _courses = new();

        // tipos simples (deixa aninhado para não criar ficheiros a mais)
        public sealed record PublicSubjectItem(int Id, string Name, string CourseName, int TotalLessons);
        public sealed record PublicCourseItem(int Id, string Name, int SubjectsCount);

        public async Task LoadAsync()
        {
            // carrega ambos para poderes alternar sem nova chamada
            var subjects = await _api.GetPublicSubjectsAsync();  // Id, Name, CourseId, TotalLessons
            var courses = await _api.GetPublicCoursesAsync();   // Id, Name

            var courseNameById = (courses ?? new()).ToDictionary(c => c.Id, c => c.Name);

            Subjects = (subjects ?? new())
                .Select(s => new PublicSubjectItem(
                    s.Id,
                    s.Name,
                    courseNameById.TryGetValue(s.CourseId, out var cn) ? cn : $"Course {s.CourseId}",
                    s.TotalLessons))
                .OrderBy(i => i.CourseName).ThenBy(i => i.Name).ToList();

            // agrupa p/ contar quantas disciplinas cada curso tem
            var countByCourse = (subjects ?? new()).GroupBy(s => s.CourseId)
                                                   .ToDictionary(g => g.Key, g => g.Count());

            Courses = (courses ?? new())
                .Select(c => new PublicCourseItem(
                    c.Id,
                    c.Name,
                    countByCourse.TryGetValue(c.Id, out var n) ? n : 0))
                .OrderBy(c => c.Name)
                .ToList();
        }
    }
}
