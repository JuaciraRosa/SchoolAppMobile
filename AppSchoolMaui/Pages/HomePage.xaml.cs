using AppSchoolMaui.Services;
using AppSchoolMaui.ViewModels;
using System.Net;

namespace AppSchoolMaui.Pages;
public partial class HomePage : ContentPage
{
    readonly HomeVm _vm;
    readonly ApiService _api;
    bool _bootstrapped; // evita rodar 2x

    public HomePage(HomeVm vm, ApiService api)
    {
        InitializeComponent();
        _vm = vm;
        _api = api;

      
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_bootstrapped)
        {
            _bootstrapped = true;
            await _api.BootstrapAuthAsync(); 
        }

        await Load();
    }

    async Task Load()
    {
        await _vm.LoadAsync();
        Hello.Text = _vm.Hello;
        List.ItemsSource = _vm.Items;
    }

    private async void OnRefresh(object s, EventArgs e) => await Load();

    private async void OnLogout(object s, EventArgs e)
    {
        try
        {
            _api.StopFeedPolling();
            await _api.LogoutAsync();
            await DisplayAlert("Logout", "Sessão terminada.", "OK");
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}