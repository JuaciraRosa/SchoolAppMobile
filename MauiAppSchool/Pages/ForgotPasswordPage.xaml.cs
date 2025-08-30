using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ApiService _api;

    public ForgotPasswordPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public ForgotPasswordPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnSend(object sender, EventArgs e)
    {
        Msg.IsVisible = Err.IsVisible = false;
        try
        {
            var email = EmailEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                Err.Text = "Email is required.";
                Err.IsVisible = true;
                return;
            }

            await _api.ForgotPasswordAsync(email);
            Msg.Text = "Check your inbox. Use the code with 'Reset password' in the app.";
            Msg.IsVisible = true;
        }
        catch (Exception ex)
        {
            Err.Text = ex.Message;
            Err.IsVisible = true;
        }
    }

    private async void OpenForgotOnWeb(object sender, EventArgs e)
    {
        var url = $"{_api.WebBase}/Account/ForgotPassword";
        await Browser.OpenAsync(url, BrowserLaunchMode.External);
    }
}