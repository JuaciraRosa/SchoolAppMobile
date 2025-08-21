using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

[QueryProperty(nameof(Email), "email")]
[QueryProperty(nameof(Token), "token")]
public partial class ResetPasswordPage : ContentPage
{
    private readonly IApiService _apiService;

    public string Email { get; set; }
    public string Token { get; set; }

    public ResetPasswordPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
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

        var result = await _apiService.ResetPasswordAsync(Email, Token, newPassword);

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

