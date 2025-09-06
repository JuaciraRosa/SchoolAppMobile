using AppSchoolMaui.Helpers;
using AppSchoolMaui.Services;
using AppSchoolMaui.ViewModels;
using System.Net;

namespace AppSchoolMaui.Pages;
public partial class MarksPage : ContentPage
{
    private readonly MarksVm _vm;
    private readonly ApiService _api;
    private readonly IAppNotifications _notify;

    public MarksPage(MarksVm vm, ApiService api, IAppNotifications notify)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _api = api;
        _notify = notify;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // garante que não tem um polling anterior ativo
        _api.StopFeedPolling();

        // inicia o polling e usa só ESTE callback
        _api.StartFeedPolling(
            TimeSpan.FromSeconds(10),
            NotifyAsync,
            initialBurst: TimeSpan.FromSeconds(30) // pode deixar 0 se quiser sem “burst”
        );

        await SafeLoadAsync(); // carrega a lista atual
        // NÃO chame ForceFeedCheckAsync aqui
    }

    protected override void OnDisappearing()
    {
        _api.StopFeedPolling();            // para quando sair da Marks
        base.OnDisappearing();
    }

    private async Task NotifyAsync(List<ApiService.FeedItem> items)
    {
        var marks = items
            .Where(i => string.Equals(i.Type, "MARK", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (marks.Count == 0) return;

        await SafeLoadAsync(); // atualiza a CollectionView

        foreach (var it in marks)
        {
            var msg = it.Value is float v
                ? $"Nova nota em {it.Subject}: {v:0.#}"
                : $"Atualização em {it.Subject}";
            await _notify.ShowAsync("Atualização", msg);
        }
    }

    private async Task SafeLoadAsync()
    {
        try { await _vm.LoadAsync(); }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Aviso", ex.ToUserMessage(), "OK");
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex) { await DisplayAlert("Erro", ex.Message, "OK"); }
    }
}
