using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class PublicPage : ContentPage
{
    private readonly ApiService _api;
    private List<ApiService.CourseDto> _courses = new();
    private List<ApiService.SubjectDto> _subjects = new();

    // construtor sem parâmetros (usado pelo XAML/Shell)
    public PublicPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public PublicPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }

    private async Task Load()
    {
        _courses = (await _api.GetCoursesAsync()).ToList();
        CoursePicker.ItemsSource = _courses.Select(c => c.Name).ToList();
        if (_courses.Count > 0) CoursePicker.SelectedIndex = 0;
        await LoadSubjects();
    }

    private async Task LoadSubjects()
    {
        var courseId = SelectedCourseId();
        _subjects = (await _api.GetSubjectsAsync(courseId)).ToList();
        SubjectsList.ItemsSource = _subjects;
    }

    private int? SelectedCourseId()
        => CoursePicker.SelectedIndex >= 0 ? _courses[CoursePicker.SelectedIndex].Id : null;

    private async void OnCourseChanged(object s, EventArgs e) => await LoadSubjects();

    private async void OnEnroll(object s, EventArgs e)
    {
        if (s is Button btn && btn.BindingContext is ApiService.SubjectDto sub)
        {
            await _api.CreateEnrollmentRequestAsync(sub.Id, note: null);
            await DisplayAlert("Success", "Request submitted.", "OK");
        }
    }

    private async void GoHome(object sender, EventArgs e)
     => await Shell.Current.GoToAsync("///home");


}