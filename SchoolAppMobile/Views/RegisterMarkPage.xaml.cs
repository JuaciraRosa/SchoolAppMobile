using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class RegisterMarkPage : ContentPage
{
    private readonly IApiService _apiService;

    public RegisterMarkPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(StudentIdEntry.Text, out int studentId) ||
            !int.TryParse(SubjectIdEntry.Text, out int subjectId) ||
            !float.TryParse(MarkValueEntry.Text, out float value))
        {
            await DisplayAlert("Error", "Please enter valid values.", "OK");
            return;
        }

        var newMark = new
        {
            StudentId = studentId,
            SubjectId = subjectId,
            Value = value,
            Date = DateTime.UtcNow
        };

        var success = await _apiService.PostAsync("staff/marks", newMark);

        if (success)
            await DisplayAlert("Success", "Mark registered.", "OK");
        else
            await DisplayAlert("Error", "Failed to register mark.", "OK");
    }
}
