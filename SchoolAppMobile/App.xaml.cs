using SchoolAppMobile.Views;
using System.Web;

namespace SchoolAppMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();


            if (string.IsNullOrEmpty(Preferences.Get("AuthToken", "")))
                Shell.Current.GoToAsync("//login");
            else
                Shell.Current.GoToAsync("//home");
        }


        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            // Ex.: escola://reset?email=a@b.com&token=XYZ
            if (uri.Scheme == "escola" && uri.Host == "reset")
            {
                var q = HttpUtility.ParseQueryString(uri.Query);
                var email = q["email"] ?? "";
                var token = q["token"] ?? "";

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var route = $"{nameof(SchoolAppMobile.Views.ResetPasswordPage)}" +
                                $"?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
                    await Shell.Current.GoToAsync(route);
                });
            }

            base.OnAppLinkRequestReceived(uri);
        }

        protected override void OnStart()
        {
            var token = Preferences.Get("jwt_token", null);
            if (!string.IsNullOrEmpty(token))
                MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.GoToAsync("//HomePage"));
        }

    }
}
