using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class ResetPasswordPage : ContentPage
{
    private readonly IApiService _apiService;
    private readonly string _email;
    private readonly string _token;

    public ResetPasswordPage(string email, string token)
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();

        _email = email;
        _token = token;
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        var newPassword = NewPasswordEntry.Text?.Trim();
        var confirm = ConfirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(newPassword) || newPassword != confirm)
        {
            await DisplayAlert("Validation", "Passwords do not match or are empty.", "OK");
            return;
        }

        var result = await _apiService.ResetPasswordAsync(_email, _token, newPassword);

        if (result)
        {
            await DisplayAlert("Success", "Password reset successfully!", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        else
        {
            await DisplayAlert("Error", "Failed to reset password. Try again.", "OK");
        }
    }
}
