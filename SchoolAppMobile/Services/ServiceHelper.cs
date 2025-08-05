using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Services
{
    public static class ServiceHelper
    {
        public static T GetService<T>() where T : class =>
            Current.GetService<T>() ?? throw new NullReferenceException($"Service {typeof(T)} not found");

        private static IServiceProvider Current =>
#if ANDROID
            MauiApplication.Current.Services;
#elif IOS || MACCATALYST
            MauiUIApplicationDelegate.Current.Services;
#elif WINDOWS
            MauiWinUIApplication.Current.Services;
#else
            throw new NotImplementedException("Platform not supported");
#endif
    }

}
