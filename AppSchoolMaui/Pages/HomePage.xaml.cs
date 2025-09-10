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

            // >>> NOVO: mostra/esconde o botão conforme login
            UpdateLogoutUi();
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Aviso", ex.ToUserMessage(), "OK");
        }
    }

    // >>> NOVO: remove/adiciona o ToolbarItem (ToolbarItem não tem IsVisible)
    void UpdateLogoutUi()
    {
        if (_vm.IsLoggedIn)
        {
            if (!ToolbarItems.Contains(LogoutItem))
                ToolbarItems.Add(LogoutItem);
        }
        else
        {
            if (ToolbarItems.Contains(LogoutItem))
                ToolbarItems.Remove(LogoutItem);
        }
    }

    private async void OnRefresh(object s, EventArgs e) => await Load();

    private async void OnLogout(object s, EventArgs e)
    {
        try
        {
            _api.StopFeedPolling();
            await _api.LogoutAsync();
            await DisplayAlert("Logout", "Sessão terminada.", "OK");

            // volta para login; quando a Home reaparecer, Load() chamará UpdateLogoutUi()
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }
}