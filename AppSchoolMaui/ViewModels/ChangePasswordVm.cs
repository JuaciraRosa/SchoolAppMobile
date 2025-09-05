using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.ViewModels
{
    public sealed class ChangePasswordVm : BaseViewModel
    {
        private readonly ApiService _api;

        public ChangePasswordVm(ApiService api) => _api = api;

        private string? _current;
        public string? Current { get => _current; set => Set(ref _current, value); }

        private string? _new1;
        public string? New1 { get => _new1; set => Set(ref _new1, value); }

        private string? _new2;
        public string? New2 { get => _new2; set => Set(ref _new2, value); }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { Set(ref _isBusy, value); ChangeCommand.ChangeCanExecute(); } }

        private string? _msg;
        public string? Msg { get => _msg; set { Set(ref _msg, value); HasMsg = !string.IsNullOrWhiteSpace(value); } }

        private bool _hasMsg;
        public bool HasMsg { get => _hasMsg; private set => Set(ref _hasMsg, value); }

        public Command ChangeCommand => new(async () => await ChangeAsync(), () => !IsBusy);

        private async Task ChangeAsync()
        {
            Msg = null;
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var cur = (Current ?? "").Trim();
                var n1 = (New1 ?? "").Trim();
                var n2 = (New2 ?? "").Trim();

                if (string.IsNullOrEmpty(cur) || string.IsNullOrEmpty(n1) || string.IsNullOrEmpty(n2))
                    throw new InvalidOperationException("Preencha todas as palavras-passe.");

                if (n1 != n2)
                    throw new InvalidOperationException("A confirmação não coincide.");

                if (n1.Length < 6)
                    throw new InvalidOperationException("A nova palavra-passe deve ter pelo menos 6 caracteres.");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await _api.ChangePasswordAsync(cur, n1, n2, cts.Token);


                // opcional: forçar re-login (muitos backends invalidam o token antigo)
                await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Palavra-passe alterada.", "OK");
                Current = New1 = New2 = string.Empty;
                Msg = null;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                Msg = "Sessão expirada. Entre novamente.";
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
    }
}
