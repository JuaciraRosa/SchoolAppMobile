using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AppSchoolMaui.ViewModels
{
    public sealed class ResetPasswordVm : BaseViewModel
    {
        private readonly ApiService _api;
        public ResetPasswordVm(ApiService api) => _api = api;

        public string? Email { get => _email; set => Set(ref _email, value); }
        public string? TokenRaw { get => _tokenRaw; set => Set(ref _tokenRaw, value); }
        public string? New1 { get => _new1; set => Set(ref _new1, value); }
        public string? New2 { get => _new2; set => Set(ref _new2, value); }
        public bool IsBusy { get => _isBusy; set { Set(ref _isBusy, value); ResetCommand.ChangeCanExecute(); } }
        public string? Msg { get => _msg; set { Set(ref _msg, value); HasMsg = !string.IsNullOrWhiteSpace(value); } }
        public bool HasMsg { get => _hasMsg; private set => Set(ref _hasMsg, value); }

        private string? _email, _tokenRaw, _new1, _new2, _msg;
        private bool _isBusy, _hasMsg;

        public Command ResetCommand => new(async () => await DoResetAsync(), () => !IsBusy);

        static string ExtractToken(string? input)
        {
            var t = (input ?? "").Trim();
            if (Uri.TryCreate(t, UriKind.Absolute, out var uri))
            {
                var q = HttpUtility.ParseQueryString(uri.Query);
                var token = q.Get("token") ?? q.Get("code") ?? q.Get("Token");
                if (!string.IsNullOrWhiteSpace(token)) return token!;
            }
            return t; // já era só o token
        }

        private async Task DoResetAsync()
        {
            Msg = null;
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var email = (Email ?? "").Trim();
                var n1 = (New1 ?? "").Trim();
                var n2 = (New2 ?? "").Trim();
                var token = ExtractToken(TokenRaw);

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) ||
                    string.IsNullOrEmpty(n1) || string.IsNullOrEmpty(n2))
                    throw new InvalidOperationException("Preencha email, token e as duas palavras-passe.");

                if (n1 != n2)
                    throw new InvalidOperationException("As palavras-passe não coincidem.");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await _api.ResetPasswordAsync(email, token, n1, n2, cts.Token);

                await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Palavra-passe redefinida. Pode iniciar sessão com a nova.", "OK");
                Email = TokenRaw = New1 = New2 = string.Empty;
                Msg = null;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                // Mostra as mensagens de validação que o backend devolve (já vem legível do teu Read<object>)
                Msg = ex.Message;
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }
            finally { IsBusy = false; }
        }
    }
}
