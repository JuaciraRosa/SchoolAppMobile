using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.ViewModels
{
    public sealed class PublicVm : BaseViewModel
    {
        private readonly ApiService _api;
        public PublicVm(ApiService api) => _api = api;

        public List<PublicSubjectItem> Items { get => _items; set => Set(ref _items, value); }
        private List<PublicSubjectItem> _items = new();

        public sealed record PublicSubjectItem(int Id, string Name, string CourseName, int TotalLessons);

        public async Task LoadAsync()
        {
            var subjects = await _api.GetPublicSubjectsAsync();       // Id, Name, CourseId, TotalLessons
            var courses = await _api.GetPublicCoursesAsync();        // Id, Name

            var courseNameById = (courses ?? new List<ApiService.CourseDto>())
                                 .ToDictionary(c => c.Id, c => c.Name);

            Items = (subjects ?? new List<ApiService.SubjectDto>())
                .Select(s => new PublicSubjectItem(
                    s.Id,
                    s.Name,
                    courseNameById.TryGetValue(s.CourseId, out var cn) ? cn : $"Course {s.CourseId}",
                    s.TotalLessons
                ))
                .OrderBy(i => i.CourseName)
                .ThenBy(i => i.Name)
                .ToList();
        }
    }
}
