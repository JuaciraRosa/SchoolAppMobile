using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly IApiService _apiService;

    public LoginPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        var loginData = new LoginDto { Email = email, Password = password };

        try
        {
            var token = await _apiService.LoginAsync(loginData);
            if (!string.IsNullOrEmpty(token))
            {
                if (RememberMeCheck.IsChecked) // ← só grava se marcado
                    Preferences.Set("jwt_token", token);

                await Shell.Current.GoToAsync("//HomePage");
            }
            else
            {
                await DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnForgotClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ForgotPasswordPage");
    }

}