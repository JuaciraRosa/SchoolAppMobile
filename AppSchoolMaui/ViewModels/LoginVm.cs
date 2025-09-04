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
            ForgotCommand = new Command(
    async () => await _api.ForgotOnWebAsync(Email?.Trim()),
    () => !IsBusy);

            GuestCommand = new Command(async () => await GuestAsync(), () => !IsBusy);
        }

        // ====== Bindables ======
        private string? _email = "";
        public string? Email
        {
            get => _email;
            set => Set(ref _email, value); // não usamos retorno
        }

        private string? _password = "";
        public string? Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                Set(ref _isBusy, value); // Set não retorna bool -> só chamamos
                                         // revalida CanExecute dos comandos sempre que IsBusy mudar
                LoginCommand.ChangeCanExecute();
                ForgotCommand.ChangeCanExecute();
                GuestCommand.ChangeCanExecute();
            }
        }

        private string? _msg;
        public string? Msg
        {
            get => _msg;
            set
            {
                Set(ref _msg, value);
                // como não temos OnPropertyChanged disponível,
                // atualizamos um backing field para o IsVisible do XAML
                HasMsg = !string.IsNullOrWhiteSpace(value);
            }
        }

        // Em vez de levantar OnPropertyChanged("HasMsg"),
        // mantemos um bool vinculado no XAML
        private bool _hasMsg;
        public bool HasMsg
        {
            get => _hasMsg;
            private set => Set(ref _hasMsg, value);
        }

        // ====== Commands ======
        public Command LoginCommand { get; }
        public Command ForgotCommand { get; }
        public Command GuestCommand { get; }

        // ====== Actions ======
        private async Task LoginAsync()
        {
            Msg = null;
            IsBusy = true;

            try
            {
                var email = Email?.Trim() ?? "";
                var pwd = Password ?? "";

                await _api.LoginAsync(email, pwd);

#if IOS
                // Pedir permissão para Alert/Sound/Badge
                var center = UserNotifications.UNUserNotificationCenter.Current;
                var settings = await center.GetNotificationSettingsAsync();
                if (settings.AuthorizationStatus != UserNotifications.UNAuthorizationStatus.Authorized)
                {
                    var result = await center.RequestAuthorizationAsync(
                        UserNotifications.UNAuthorizationOptions.Alert
                        | UserNotifications.UNAuthorizationOptions.Sound
                        | UserNotifications.UNAuthorizationOptions.Badge
                    );
                    // opcional: checar result.Item1 (granted)
                }
#endif

                // liga polling dos avisos
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

                // navega para a TabBar principal (rota definida no Shell como //app)
                await Shell.Current.GoToAsync("//app");
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GuestAsync()
        {
            try
            {
                await _api.LogoutAsync();
                await Shell.Current.GoToAsync("//app");
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }
        }
    }


}
