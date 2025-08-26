using MauiAppSchool.Helpers;
using MauiAppSchool.Services;
using Microsoft.Extensions.Logging;

namespace MauiAppSchool
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            const string ApiBase = "https://escolainfosysapi.somee.com";
            builder.Services.AddSingleton(sp =>
            {
                var svc = new ApiService(ApiBase);
                _ = svc.LoadTokenAsync();
                return svc;
            });

            // DI das páginas
            builder.Services.AddTransient<Pages.LoginPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<Pages.HomePage>();
            builder.Services.AddTransient<Pages.MarksPage>();
            builder.Services.AddTransient<Pages.AbsencesPage>();
            builder.Services.AddTransient<Pages.StatusPage>();
            builder.Services.AddTransient<Pages.ProfilePage>();
            builder.Services.AddTransient<Pages.PublicPage>();

            // constrói o app primeiro, depois inicializa o ServiceHelper
            var app = builder.Build();
            ServiceHelper.Initialize(app.Services);

            return builder.Build();
        }
    }
}
