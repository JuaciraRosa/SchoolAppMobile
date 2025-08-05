using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class StaffStudentsPage : ContentPage
{
    private readonly IApiService _apiService;

    public StaffStudentsPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadStudentsAsync();
    }

    private async void LoadStudentsAsync()
    {
        var students = await _apiService.GetListAsync<StudentDto>("staff/students");
        StudentsListView.ItemsSource = students;
    }

    private async void OnStudentSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is StudentDto student)
        {
            await Shell.Current.GoToAsync($"StudentDetailsPage?studentId={student.Id}");
        }
    }
}