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
        private readonly IAppNotifications _notify;

        public LoginVm(ApiService api, IAppNotifications notify)
        {
            _api = api;
            _notify = notify;

            LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
            ForgotCommand = new Command(async () => await _api.ForgotOnWebAsync(Email?.Trim()), () => !IsBusy);
            GuestCommand = new Command(async () => await GuestAsync(), () => !IsBusy);
        }

        // ====== Bindables ======
        private string? _email = "";
        public string? Email { get => _email; set => Set(ref _email, value); }

        private string? _password = "";
        public string? Password { get => _password; set => Set(ref _password, value); }

        private bool _isBusy;
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

        private string? _msg;
        public string? Msg
        {
            get => _msg;
            set { Set(ref _msg, value); HasMsg = !string.IsNullOrWhiteSpace(value); }
        }

        private bool _hasMsg;
        public bool HasMsg { get => _hasMsg; private set => Set(ref _hasMsg, value); }

        // ====== Commands ======
        public Command LoginCommand { get; }
        public Command ForgotCommand { get; }
        public Command GuestCommand { get; }

        // ====== Actions ======
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
        var center = UserNotifications.UNUserNotificationCenter.Current;
        var settings = await center.GetNotificationSettingsAsync();
        if (settings.AuthorizationStatus != UserNotifications.UNAuthorizationStatus.Authorized)
        {
            var result = await center.RequestAuthorizationAsync(
                UserNotifications.UNAuthorizationOptions.Alert
                | UserNotifications.UNAuthorizationOptions.Sound
                | UserNotifications.UNAuthorizationOptions.Badge);
        }
#endif

                _api.StartFeedPolling(TimeSpan.FromSeconds(10), async items =>
                {
                    foreach (var it in items)
                    {
                        var text = it.Type?.ToUpperInvariant() == "MARK"
                            ? $"Nova nota em {it.Subject}: {it.Value}"
                            : $"Atualização em {it.Subject}";
                        await _notify.ShowAsync("Atualização", text);
                    }
                });

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
                await _api.LogoutAsync();          // remove token/Authorization
                Preferences.Set("guest", true);    // modo convidado
                await Shell.Current.GoToAsync("//public"); // <- vai para página pública
            }
            catch (Exception ex) { Msg = ex.Message; }
        }

    }



}
