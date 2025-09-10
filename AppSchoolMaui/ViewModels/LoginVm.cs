using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.ViewModels
{
   
        public sealed class LoginVm : BaseViewModel
        {
            private readonly ApiService _api;

            public LoginVm(ApiService api)
            {
                _api = api;
                LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
                ForgotCommand = new Command(async () => await _api.ForgotOnWebAsync(Email?.Trim()), () => !IsBusy);
                GuestCommand = new Command(async () => await GuestAsync(), () => !IsBusy);
            }

            public string? Email { get => _email; set => Set(ref _email, value); }
            private string? _email = "";

            public string? Password { get => _password; set => Set(ref _password, value); }
            private string? _password = "";

            public bool IsBusy
            {
                get => _isBusy;
                set
                {
                    Set(ref _isBusy, value);
                    LoginCommand.ChangeCanExecute();
                    ForgotCommand.ChangeCanExecute();
                    GuestCommand.ChangeCanExecute();
                }
            }
            private bool _isBusy;

            public string? Msg { get => _msg; set { Set(ref _msg, value); HasMsg = !string.IsNullOrWhiteSpace(value); } }
            private string? _msg;
            public bool HasMsg { get => _hasMsg; private set => Set(ref _hasMsg, value); }
            private bool _hasMsg;

            public Command LoginCommand { get; }
            public Command ForgotCommand { get; }
            public Command GuestCommand { get; }

            private async Task LoginAsync()
            {
                Msg = null;
                if (IsBusy) return;
                IsBusy = true;

                try
                {
                    var email = (Email ?? "").Trim();
                    var pwd = Password ?? "";

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    await _api.LoginAsync(email, pwd, cts.Token);

                    Preferences.Set("guest", false); // logado

#if IOS
                var center   = UserNotifications.UNUserNotificationCenter.Current;
                var settings = await center.GetNotificationSettingsAsync();
                if (settings.AuthorizationStatus != UserNotifications.UNAuthorizationStatus.Authorized)
                {
                    _ = await center.RequestAuthorizationAsync(
                        UserNotifications.UNAuthorizationOptions.Alert
                      | UserNotifications.UNAuthorizationOptions.Sound
                      | UserNotifications.UNAuthorizationOptions.Badge);
                }
#endif
                    // NADA de StartFeedPolling aqui.
                    await Shell.Current.GoToAsync("//app");
                }
                catch (HttpRequestException ex) { Msg = ex.Message; }
                catch (Exception ex) { Msg = ex.Message; }
                finally { IsBusy = false; }
            }

        private async Task GuestAsync()
        {
            try
            {
                _api.StopFeedPolling();       
                await _api.LogoutAsync();      
                Preferences.Set("guest", true);

                // limpa o back stack e força público
                await Shell.Current.GoToAsync("//public", true);
            }
            catch (Exception ex) { Msg = ex.Message; }
        }

    }

}
