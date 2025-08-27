using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _api;

 
    public LoginPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public LoginPage(ApiService api) { InitializeComponent(); _api = api; }

    private async void OnLogin(object s, EventArgs e)
    {
        Msg.IsVisible = false;
        try
        {
            await _api.LoginAsync(EmailEntry.Text?.Trim() ?? "", PasswordEntry.Text ?? "");
            (Application.Current as App)!.GoToShell();
        }
        catch (Exception ex) { Msg.Text = ex.Message; Msg.IsVisible = true; }
    }


    // Forgot: pede email e chama o endpoint
    private async void OnForgotPassword(object sender, EventArgs e)
    {
        var email = await DisplayPromptAsync("Forgot password", "Enter your email:");
        if (string.IsNullOrWhiteSpace(email)) return;

        try
        {
            await _api.ForgotPasswordAsync(email.Trim());
            await DisplayAlert("Sent",
                "If the email exists, a reset token/link has been generated.",
                "OK");
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }


    private async void OnResetPassword(object sender, EventArgs e)
    {
        var email = await DisplayPromptAsync("Reset password", "Email:");
        if (string.IsNullOrWhiteSpace(email)) return;

        var token = await DisplayPromptAsync("Reset password", "Token (paste the code):");
        if (string.IsNullOrWhiteSpace(token)) return;
        var newPwd = await DisplayPromptAsync(
            "Reset password",
            "New password:",
            accept: "OK",
            cancel: "Cancel",
            placeholder: "Enter new password",
            maxLength: -1,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(newPwd)) return;

        try
        {
            await _api.ResetPasswordAsync(email.Trim(), token.Trim(), newPwd);
            await DisplayAlert("Done", "Password updated. You can log in now.", "OK");
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }



    private async void OnGuest(object sender, EventArgs e)
    {
        await _api.UseAnonymousAsync(); // limpa token
                                        // P�gina dedicada ao an�nimo (n�o usa Shell/abas privadas)
        Application.Current!.MainPage = new NavigationPage(new Pages.GuestPage());
    }


}