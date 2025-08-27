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
            const string WebBase = "https://escolainfosys.somee.com"; // fotos/estáticos (MVC)

            // somente ESTA inscrição
            builder.Services.AddSingleton(sp =>
            {
                var svc = new ApiService(ApiBase, webBase: WebBase);
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

            // ✅ constrói uma vez e retorna o mesmo 'app'
            var app = builder.Build();
            ServiceHelper.Initialize(app.Services);
            return app;
        }
    }
}
