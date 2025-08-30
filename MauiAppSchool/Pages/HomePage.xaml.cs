using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _api;

    // construtor sem parâmetros (usado pelo XAML/Shell)
    public HomePage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public HomePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }

    private async Task Load()
    {
        var me = await _api.GetProfileAsync();
        Hello.Text = $"Hello, {me.FullName}";
        var feed = await _api.GetFeedAsync(DateTime.UtcNow.AddDays(-30));
        FeedList.ItemsSource = feed.items;


    }



    private async void OnRefresh(object s, EventArgs e) => await Load();
    private async void OpenPublic(object s, EventArgs e) => await Shell.Current.GoToAsync(nameof(PublicPage));

    private async void OnLogout(object s, EventArgs e)
    {
        var confirm = await DisplayAlert("Logout", "End your session?", "Yes", "No");
        if (!confirm) return;

        try
        {
            await _api.LogoutAsync();   // limpa token
            _api.StopFeedPolling();     // desliga avisos do feed
            Application.Current!.MainPage = new NavigationPage(new Pages.LoginPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }



}