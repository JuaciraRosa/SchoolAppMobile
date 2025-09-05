using AppSchoolMaui.Services;
using AppSchoolMaui.ViewModels;
using System.Net;

namespace AppSchoolMaui.Pages;
public partial class HomePage : ContentPage
{
    readonly HomeVm _vm;
    readonly ApiService _api;
    readonly IAppNotifications _notify;  

    public HomePage(HomeVm vm, ApiService api, IAppNotifications notify) 
    {
        InitializeComponent();
        _vm = vm;
        _api = api;
        _notify = notify;
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
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    // callback de notificação (mesma lógica do LoginVm)
    private async Task NotifyAsync(List<ApiService.FeedItem> items)
    {
        foreach (var it in items)
        {
            var text = (it.Type ?? "").ToUpperInvariant() == "MARK"
                ? $"Nova nota em {it.Subject}: {it.Value}"
                : $"Atualização em {it.Subject}";
            await _notify.ShowAsync("Atualização", text);
        }
    }

    private async void OnRefresh(object s, EventArgs e)
    {
        // 1) verifica o feed AGORA (sem esperar o timer)
        await _api.ForceFeedCheckAsync(NotifyAsync);

        // 2) recarrega a lista da Home
        await Load();
    }

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