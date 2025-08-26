using MauiAppSchool.Services;

namespace MauiAppSchool
{
    public partial class App : Application
    {
        private readonly IServiceProvider _sp;

        public App(IServiceProvider sp)
        {
            InitializeComponent();
            _sp = sp;

            // Login inicia fora do Shell
            MainPage = new NavigationPage(new Pages.LoginPage()); // usa ctor sem parâmetros
        }

        public void GoToShell()
        {
            // ✅ garante que a troca de MainPage roda na UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var shell = ActivatorUtilities.CreateInstance<AppShell>(_sp);
                Application.Current.MainPage = shell;


                // (opcional) garante a aba Home selecionada
                _ = Shell.Current.GoToAsync("///home");
            });
        }
    }
}
