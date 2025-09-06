using Android.App;
using Android.Content.PM;
using Android.OS;

namespace AppSchoolMaui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Exported = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    // 👇 ESTE é o filtro que associa o teu domínio/rota à app
    [IntentFilter(
        new[] { Android.Content.Intent.ActionView },
        Categories = new[] {
            Android.Content.Intent.CategoryDefault,
            Android.Content.Intent.CategoryBrowsable
        },
        DataScheme = "https",
        DataHost = "www.escolainfosys.somee.com",
        DataPathPrefix = "/Account/ResetPassword")]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
