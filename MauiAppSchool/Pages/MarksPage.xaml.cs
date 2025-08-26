using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class MarksPage : ContentPage
{
    private readonly ApiService _api;

    // construtor sem parâmetros (usado pelo XAML/Shell)
    public MarksPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public MarksPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }
    private async Task Load() => List.ItemsSource = await _api.GetMarksAsync();



    private async void GoHome(object sender, EventArgs e)
    => await Shell.Current.GoToAsync("///home");

}