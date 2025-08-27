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



}