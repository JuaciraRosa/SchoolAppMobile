using AppSchoolMaui.Pages;
using AppSchoolMaui.Services;

namespace AppSchoolMaui
{
    public partial class AppShell : Shell
    {
        private readonly ApiService _api;
        bool _navigatedOnce;

        public AppShell(ApiService api)
        {
            InitializeComponent();
            _api = api;

            Loaded += async (_, __) =>
            {
                if (_navigatedOnce) return;
                _navigatedOnce = true;

                await _api.BootstrapAuthAsync(); 

                var token = await SecureStorage.GetAsync("token");
                await GoToAsync(string.IsNullOrWhiteSpace(token) ? "//login" : "//app");
            };
        }
    }

}
