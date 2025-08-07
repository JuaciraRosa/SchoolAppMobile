using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class SendEnrollmentPage : ContentPage
{
    private readonly IApiService _apiService;

    public SendEnrollmentPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
    }
    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var message = MessageEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            var confirm = await DisplayAlert("Empty message", "Do you want to send the request without a message?", "Yes", "No");
            if (!confirm) return;
        }

        var success = await _apiService.PostAsync("enrollmentrequests", new CreateEnrollmentRequestDto
        {
            Message = message
        });

        if (success)
        {
            await DisplayAlert("Success", "Your enrollment request was submitted.", "OK");
            await Shell.Current.GoToAsync("///HomePage");
        }
        else
        {
            await DisplayAlert("Error", "Failed to send the request. Try again later.", "OK");
        }
    }





    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }

}