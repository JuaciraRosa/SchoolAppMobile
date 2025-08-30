using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class ResetPasswordPage : ContentPage
{
    private readonly ApiService _api;

    public ResetPasswordPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public ResetPasswordPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnReset(object sender, EventArgs e)
    {
        Msg.IsVisible = Err.IsVisible = false;
        try
        {
            var email = EmailEntry.Text?.Trim();
            var code = CodeEntry.Text?.Trim();
            var pwd = NewPwdEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(pwd))
            {
                Err.Text = "Email, code and new password are required.";
                Err.IsVisible = true;
                return;
            }

            await _api.ResetPasswordAsync(email, code, pwd);
            Msg.Text = "Password updated. You can now sign in.";
            Msg.IsVisible = true;
        }
        catch (Exception ex)
        {
            Err.Text = ex.Message;
            Err.IsVisible = true;
        }
    }
}