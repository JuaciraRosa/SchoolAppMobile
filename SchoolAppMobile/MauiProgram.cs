using Microsoft.Extensions.Logging;
using SchoolAppMobile.Services;
using SchoolAppMobile.Views;

namespace SchoolAppMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Serviços
            builder.Services.AddSingleton<IApiService, ApiService>();

   
            builder.Services.AddSingleton<SendEnrollmentPage>();
            builder.Services.AddSingleton<ExplorePage>();
            builder.Services.AddSingleton<PublicSubjectsPage>();
            builder.Services.AddSingleton<SubjectContentsPage>();

       
            builder.Services.AddSingleton<MarksPage>();
            builder.Services.AddSingleton<AbsencesPage>();
            builder.Services.AddSingleton<NotificationsPage>();
            builder.Services.AddSingleton<ProfilePage>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<HomePage>();

            return builder.Build();
        }
    }

}
