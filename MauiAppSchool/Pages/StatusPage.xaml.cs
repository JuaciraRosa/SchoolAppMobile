using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class StatusPage : ContentPage
{
    private readonly ApiService _api;

    // construtor sem parâmetros (usado pelo XAML/Shell)
    public StatusPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public StatusPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }

    private async void GoHome(object sender, EventArgs e)
    => await Shell.Current.GoToAsync("///home");

    private async Task Load() => List.ItemsSource = await _api.GetStatusPerSubjectAsync();
}