using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Webkit;
using Microsoft.Maui.Controls;

namespace SchoolAppMobile
{
    [Activity(
      Theme = "@style/Maui.SplashTheme",
      MainLauncher = true,
      ConfigurationChanges = ConfigChanges.ScreenSize
                            | ConfigChanges.Orientation
                            | ConfigChanges.UiMode
                            | ConfigChanges.ScreenLayout
                            | ConfigChanges.SmallestScreenSize
                            | ConfigChanges.Density)]

    // 👇 Usa o nome totalmente qualificado: Android.App.IntentFilter
    [Android.App.IntentFilter(
      new[] { Android.Content.Intent.ActionView },
      Categories = new[]
      {
        Android.Content.Intent.CategoryDefault,
        Android.Content.Intent.CategoryBrowsable
      },
      DataScheme = "escola",
      DataHost = "reset"
  )]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var data = this.Intent?.Data;
            if (data != null)
            {
                // 👇 aqui forço para o Application do MAUI, não o do Android
                Microsoft.Maui.Controls.Application.Current?
                    .SendOnAppLinkRequestReceived(new Uri(data.ToString()));
            }
        }

        protected override void OnNewIntent(Android.Content.Intent? intent)
        {
            base.OnNewIntent(intent);

            var data = intent?.Data;
            if (data != null)
            {
                Microsoft.Maui.Controls.Application.Current?
                    .SendOnAppLinkRequestReceived(new Uri(data.ToString()));
            }
        }
    }
}