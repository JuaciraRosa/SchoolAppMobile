using AppSchoolMaui.Services;

namespace AppSchoolMaui
{
    public partial class App : Application
    {
        private readonly ApiService _api;
        private readonly IAppNotifications _notify;

        public App(ApiService api, IAppNotifications notify)
        {
            InitializeComponent();
            _api = api; _notify = notify;

            MainPage = new AppShell(_api);
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            await _api.BootstrapAuthAsync();
            if (_api.HasAuth)
                _api.StartFeedPolling(TimeSpan.FromSeconds(10), OnFeedAsync);
        }

        protected override void OnSleep() => _api.StopFeedPolling();

        protected override void OnResume()
        {
            if (_api.HasAuth)
                _api.StartFeedPolling(TimeSpan.FromSeconds(10), OnFeedAsync);
        }

        private async Task OnFeedAsync(List<ApiService.FeedItem> items)
        {
            foreach (var it in items)
            {
                var text = it.Type?.ToUpperInvariant() == "MARK"
                    ? $"Nova nota em {it.Subject}: {it.Value}"
                    : $"Atualização em {it.Subject}";
                await _notify.ShowAsync("Atualização", text);
            }
        }
    }


}
