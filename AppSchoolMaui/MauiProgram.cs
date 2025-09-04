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

            // DI
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
           


#if DEBUG
            builder.Logging.AddDebug();
#endif

#if IOS
            // zera o badge do ícone ao iniciar no iOS
#pragma warning disable CA1422
            UIKit.UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
#pragma warning restore CA1422
#endif
            return builder.Build();
        }
    }
}
