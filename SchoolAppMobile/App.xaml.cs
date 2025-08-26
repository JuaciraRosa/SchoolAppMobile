using EscolaInfoSys.Mobile.Services;
using SchoolAppMobile.Services;
using static System.Net.Mime.MediaTypeNames;

public partial class App : Application
{
    private readonly ApiService _api;
    private readonly IServiceProvider _sp;

    public App(ApiService api, IServiceProvider sp)
    {
        InitializeComponent();
        _api = api;
        _sp = sp;

        // começa no login
        MainPage = new NavigationPage(ActivatorUtilities.CreateInstance<LoginPage>(_sp));
    }

    public void GoToShell()
    {
        // cria o Shell via DI
        var shell = ActivatorUtilities.CreateInstance<AppShell>(_sp);
        MainPage = shell;
    }
}
