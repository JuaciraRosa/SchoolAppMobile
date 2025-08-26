using EscolaInfoSys.Mobile.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        const string ApiBase = "https://escolainfosysapi.somee.com"; // troque p/ http:// se o emulador não aceitar TLS
        builder.Services.AddSingleton(sp =>
        {
            var svc = new ApiService(ApiBase);
            _ = svc.LoadTokenAsync();
            return svc;
        });

        // registra páginas para DI
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<MarksPage>();
        builder.Services.AddTransient<AbsencesPage>();
        builder.Services.AddTransient<StatusPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<PublicPage>();

        return builder.Build();
    }
}

