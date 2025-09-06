using AppSchoolMaui.Helpers;
using AppSchoolMaui.Services;
using AppSchoolMaui.ViewModels;
using System.Net;

namespace AppSchoolMaui.Pages;
public partial class HomePage : ContentPage
{
    readonly HomeVm _vm;
    readonly ApiService _api;

    public HomePage(HomeVm vm, ApiService api)
    {
        InitializeComponent();
        _vm = vm;
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Load();
    }

    async Task Load()
    {
        try
        {
            await _vm.LoadAsync();
            Hello.Text = _vm.Hello;
            List.ItemsSource = _vm.Items;
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Aviso", ex.ToUserMessage(), "OK");
        }
    }

    private async void OnRefresh(object s, EventArgs e) => await Load();

    private async void OnLogout(object s, EventArgs e)
    {
        try
        {
            _api.StopFeedPolling();
            await _api.LogoutAsync();
            await DisplayAlert("Logout", "Sess�o terminada.", "OK");
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}