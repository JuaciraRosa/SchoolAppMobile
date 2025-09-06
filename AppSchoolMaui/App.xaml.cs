using AppSchoolMaui.Pages;
using AppSchoolMaui.Services;

namespace AppSchoolMaui
{
    public partial class App : Application
    {
        private readonly ApiService _api;

        public App(ApiService api)   
        {
            InitializeComponent();
            _api = api;

        
            MainPage = new AppShell(_api);
        }

      
        protected override void OnSleep()
        {
            _api.StopFeedPolling();
            base.OnSleep();
        }


    
    }


}
