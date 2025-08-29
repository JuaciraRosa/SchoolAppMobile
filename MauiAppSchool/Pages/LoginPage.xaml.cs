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


    private async void OpenForgotOnWeb(object s, EventArgs e)
    {
        var url = $"{_api.WebBase}/Account/ForgotPassword";
        await Browser.OpenAsync(new Uri(url), BrowserLaunchMode.External);
    }




    private async void OnGuest(object sender, EventArgs e)
    {
        await _api.UseAnonymousAsync(); // limpa token
                                        // Página dedicada ao anónimo (não usa Shell/abas privadas)
        Application.Current!.MainPage = new NavigationPage(new Pages.GuestPage());
    }


}