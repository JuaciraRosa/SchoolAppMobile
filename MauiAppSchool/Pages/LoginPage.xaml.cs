using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _api;

    // construtor sem parâmetros (usado pelo XAML/Shell)
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
}