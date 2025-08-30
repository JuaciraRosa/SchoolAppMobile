using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _api;

    // ====== CONSTRUTORES (mantidos) ======
    public LoginPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public LoginPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    // ====== LOGIN (mantido) ======
    private async void OnLogin(object s, EventArgs e)
    {
        Msg.IsVisible = false;

        try
        {
            await _api.LoginAsync(EmailEntry.Text?.Trim() ?? "", PasswordEntry.Text ?? "");

            // LIGA os avisos (polling do /api/feed a cada 30s)
            _api.StartFeedPolling(TimeSpan.FromSeconds(8), items =>
            {
                foreach (var it in items)
                {
                    var msg =
                        it.Type?.Equals("mark", StringComparison.OrdinalIgnoreCase) == true
                            ? $"Nova nota em {it.Subject}: {it.Value}"
                        : it.Type?.Equals("status", StringComparison.OrdinalIgnoreCase) == true
                            ? $"Estado atualizado em {it.Subject}"
                        : $"Atualização em {it.Subject}";

                    MainThread.BeginInvokeOnMainThread(async () =>
                        await Shell.Current.DisplayAlert("Aviso", msg, "OK"));
                }
            });

            (Application.Current as App)!.GoToShell();
        }
        catch (Exception ex)
        {
            Msg.Text = ex.Message;
            Msg.IsVisible = true;
        }
    }


    // ====== ESQUECI (abre no site) — ORIGINAL, mantido ======
    private async void OpenForgotOnWeb(object s, EventArgs e)
    {
        var url = $"{_api.WebBase}/Account/ForgotPassword";
        await Browser.OpenAsync(new Uri(url), BrowserLaunchMode.External);
    }

    // ====== CONVIDADO (mantido) ======
    private async void OnGuest(object sender, EventArgs e)
    {
        await _api.UseAnonymousAsync(); // limpa token
        Application.Current!.MainPage = new NavigationPage(new Pages.GuestPage());
    }

    private async void OnLogout(object s, EventArgs e)
    {
        try
        {
            await _api.LogoutAsync();   // limpa o token
            _api.StopFeedPolling();     // desliga os avisos
            await DisplayAlert("Logout", "Sessão terminada.", "OK");
            // já está na LoginPage, então não precisa navegar
        }
        catch (Exception ex)
        {
            Msg.Text = ex.Message;
            Msg.IsVisible = true;
        }
    }


    // ====== NOVO: ESQUECI (pela app) — opcional, não remove o original ======
    private async void OnForgotPassword(object s, EventArgs e)
    {
        try
        {
            var email = await DisplayPromptAsync("Forgot password", "Enter your account email:");
            if (string.IsNullOrWhiteSpace(email)) return;

            await _api.ForgotPasswordAsync(email.Trim());

            await DisplayAlert(
                "Check your inbox",
                "We sent you a reset link and a code. Use 'Reset password (I have a code)' to finish here in the app.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void GoForgotInApp(object s, EventArgs e)
    {
        // precisa que LoginPage esteja dentro de uma NavigationPage
        await Navigation.PushAsync(new Pages.ForgotPasswordPage());
    }

    private async void GoResetInApp(object s, EventArgs e)
    {
        await Navigation.PushAsync(new Pages.ResetPasswordPage());
    }

    // ====== NOVO: RESET (pela app, com code do email) ======
    private async void OnResetPassword(object s, EventArgs e)
    {
        try
        {
            var email = await DisplayPromptAsync("Reset password", "Email:");
            if (string.IsNullOrWhiteSpace(email)) return;

            var code = await DisplayPromptAsync("Reset password", "Paste the code you received by email:");
            if (string.IsNullOrWhiteSpace(code)) return;

            // Se quiser entrada mascarada, crie uma mini página com Entry IsPassword="True"
            var newPwd = await DisplayPromptAsync("Reset password", "New password:");
            if (string.IsNullOrWhiteSpace(newPwd)) return;

            await _api.ResetPasswordAsync(email.Trim(), code.Trim(), newPwd);
            await DisplayAlert("Done", "Password updated. You can now sign in.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}