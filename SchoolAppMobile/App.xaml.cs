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
    }
}
