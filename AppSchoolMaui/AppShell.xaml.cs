using AppSchoolMaui.Pages;
using AppSchoolMaui.Services;

namespace AppSchoolMaui
{
    public partial class AppShell : Shell
    {
        private readonly ApiService _api;
        bool _ran;

        public AppShell(ApiService api)
        {
            InitializeComponent();
            _api = api;

          
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

        }
    }


}
