using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class SendEnrollmentPage : ContentPage
{
    private readonly IApiService _api;

    public SendEnrollmentPage()
    {
        InitializeComponent();
        _api = ServiceHelper.GetService<IApiService>();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var text = MessageEditor.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            await DisplayAlert("Validation", "Please enter your request.", "OK");
            return;
        }

        // Precisa de JWT (endpoint é [Authorize(Roles="Student")])
        var ok = await _api.PostAsync("enrollmentrequests", new { message = text });
        if (ok)
        {
            await DisplayAlert("Success", "Enrollment request submitted.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }
}