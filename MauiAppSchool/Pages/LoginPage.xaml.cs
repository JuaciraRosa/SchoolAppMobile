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


    private async void OnForgotPassword(object sender, EventArgs e)
    {
        var email = await DisplayPromptAsync("Forgot password", "Enter your email:");
        if (string.IsNullOrWhiteSpace(email)) return;

        try
        {
            var link = await _api.ForgotPasswordGetLinkAsync(email.Trim());

            if (!string.IsNullOrWhiteSpace(link))
            {
                await Launcher.Default.OpenAsync(new Uri(link));
            }
            else
            {
                await DisplayAlert("Sent",
                    "If the email exists, we sent you a reset link. Please check your inbox.",
                    "OK");
            }
        }
        catch (Exception ex)
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
                                        // Página dedicada ao anónimo (não usa Shell/abas privadas)
        Application.Current!.MainPage = new NavigationPage(new Pages.GuestPage());
    }


}