namespace AppSchoolMaui
{
    public partial class App : Application
    {
        public App(AppShell shell)
        {
            InitializeComponent();
            MainPage = shell; // criado pelo DI, shell recebe ApiService
        }
    }

}
