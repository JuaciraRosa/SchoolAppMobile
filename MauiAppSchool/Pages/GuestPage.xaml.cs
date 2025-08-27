using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class GuestPage : ContentPage
{
    private readonly ApiService _api;
    private List<ApiService.CourseDto> _courses = new();
    private List<ApiService.SubjectDto> _subjects = new();

    public GuestPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public GuestPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }

    private async Task Load()
    {
        try
        {
            var info = await _api.GetSystemInfoAsync();
            SysInfo.Text = $"{info.App} v{info.Version}";
        }
        catch { SysInfo.Text = "API online"; }

        _courses = (await _api.GetCoursesAsync()).ToList();
        CoursePicker.ItemsSource = _courses.Select(c => c.Name).ToList();
        if (_courses.Count > 0) CoursePicker.SelectedIndex = 0;
        await LoadSubjects();
    }

    private int? SelectedCourseId()
        => CoursePicker.SelectedIndex >= 0 ? _courses[CoursePicker.SelectedIndex].Id : null;

    private async Task LoadSubjects()
    {
        var courseId = SelectedCourseId();
        _subjects = (await _api.GetSubjectsAsync(courseId)).ToList();
        SubjectsList.ItemsSource = _subjects;
    }

    private async void OnCourseChanged(object sender, EventArgs e) => await LoadSubjects();

    private async void GoLogin(object sender, EventArgs e)
    {
        // volta para o ecrã de login
        Application.Current!.MainPage = new NavigationPage(new Pages.LoginPage());
        await Task.CompletedTask;
    }
}