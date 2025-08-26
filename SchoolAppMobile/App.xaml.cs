using SchoolAppMobile.Services;
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
            MainThread.BeginInvokeOnMainThread(RouteOnLaunch);
        }

        private async void RouteOnLaunch()
        {
            var token = Preferences.Get("jwt_token", null);
            var api = ServiceHelper.GetService<IApiService>();

            if (string.IsNullOrEmpty(token))
                await Shell.Current.GoToAsync("//LoginPage");
            else
            {
                api.SetAuthToken(token);
                await Shell.Current.GoToAsync("//HomePage");
            }
        }
    }


}
