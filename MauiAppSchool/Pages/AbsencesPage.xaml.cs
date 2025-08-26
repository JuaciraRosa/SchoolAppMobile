using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class AbsencesPage : ContentPage
{
    private readonly ApiService _api;

    // construtor sem parâmetros (usado pelo XAML/Shell)
    public AbsencesPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public AbsencesPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }

    private async Task Load()
    {
        var data = await _api.GetAbsencesAsync();
        SummaryList.ItemsSource = data.summary.perSubject;
        ItemsList.ItemsSource = data.items;
    }

    private async void GoHome(object sender, EventArgs e)
=> await Shell.Current.GoToAsync("///home");

}