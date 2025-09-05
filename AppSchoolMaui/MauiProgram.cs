using AppSchoolMaui.Pages;
using AppSchoolMaui.Services;
using AppSchoolMaui.ViewModels;
using Microsoft.Extensions.Logging;




namespace AppSchoolMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.UseMauiApp<App>();

            // DI (o teu bloco atual)
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<IAppNotifications, InAppNotifier>();

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<PublicPage>();
            builder.Services.AddSingleton<ProfilePage>();
            builder.Services.AddSingleton<MarksPage>();
            builder.Services.AddSingleton<AbsencesPage>();
            builder.Services.AddSingleton<StatusPage>();

            builder.Services.AddSingleton<LoginVm>();
            builder.Services.AddSingleton<HomeVm>();
            builder.Services.AddSingleton<PublicVm>();
            builder.Services.AddSingleton<ProfileVm>();
            builder.Services.AddSingleton<MarksVm>();
            builder.Services.AddSingleton<AbsencesVm>();
            builder.Services.AddSingleton<StatusVm>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<EnrollmentRequestsVm>();
            builder.Services.AddSingleton<EnrollmentRequestsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if IOS
#pragma warning disable CA1422
    UIKit.UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
#pragma warning restore CA1422
#endif

            // ⬇️ AQUI: inicializa o ServiceHelper e só depois retorna
            var app = builder.Build();
            ServiceHelper.Initialize(app.Services);
            return app;
        }

    }
}
