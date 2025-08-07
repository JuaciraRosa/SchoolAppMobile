using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly IApiService _apiService;

    public ForgotPasswordPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Error", "Please enter your email.", "OK");
            return;
        }

        var success = await _apiService.ForgotPasswordAsync(email);

        if (success)
        {
            await DisplayAlert("Success", "Reset link sent to your email.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Unable to send reset email. Check your email and try again.", "OK");
        }
    }
}
