using AppSchoolMaui.Pages;
using AppSchoolMaui.Services;

namespace AppSchoolMaui
{
    public partial class AppShell : Shell
    {
        private readonly ApiService _api;
        bool _ran;

        bool IsLoggedIn() =>
            _api.HasAuth && !Preferences.Get("guest", false);

    
        static readonly string[] PublicRoots =
        {
        "//login",
        "//public",
        nameof(LoginPage),
        nameof(PublicPage),
        nameof(AboutPage),          
       
    };

        bool RequiresAuth(Uri location)
        {
            var route = location.OriginalString?.Trim() ?? "";
            return !PublicRoots.Any(p =>
                route.StartsWith(p, StringComparison.OrdinalIgnoreCase) ||
                route.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        public AppShell(ApiService api)
        {
            InitializeComponent();
            _api = api;

          
            Navigating += async (_, e) =>
            {
                try
                {
                    if (e.Target is null) return;

                    if (RequiresAuth(e.Target.Location) && !IsLoggedIn())
                    {
                        e.Cancel();
                        await DisplayAlert("Área restrita", "Inicie sessão para continuar.", "OK");
                        await GoToAsync("//login");
                    }
                }
                catch { /* silencia para não quebrar navegação */ }
            };

        
            Loaded += (_, __) => Dispatcher.Dispatch(async () =>
            {
                if (_ran) return; _ran = true;

                try
                {
                    var boot = _api.BootstrapAuthAsync();
                    await Task.WhenAny(boot, Task.Delay(1500));

                    var token = await SecureStorage.GetAsync("token");
                    await GoToAsync(string.IsNullOrWhiteSpace(token) ? "//login" : "//app");
                }
                catch
                {
                    await GoToAsync("//login");
                }
            });

          
            Routing.RegisterRoute(nameof(EnrollmentRequestsPage), typeof(EnrollmentRequestsPage));
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
            Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
        }
    }
}