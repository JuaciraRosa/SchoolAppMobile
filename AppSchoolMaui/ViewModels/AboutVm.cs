using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppSchoolMaui.ViewModels
{
   public sealed class AboutVm : BaseViewModel
    {
        public string AppName     => AppInfo.Current.Name ?? "Escola InfoSys App Móvel";
        public string Version     => AppInfo.Current.VersionString;
        public string Build       => AppInfo.Current.BuildString;
        public string Author      => "Juacira Rosa";
        public string ReleaseDate => "2025-09-11";

        public ICommand OpenWebsiteCommand { get; }
        public ICommand CopyVersionCommand { get; }

        public AboutVm()
        {
            OpenWebsiteCommand = new Command(async () => await OpenWebsiteAsync());

            CopyVersionCommand = new Command(async () =>
            {
                await Clipboard.SetTextAsync($"{Version} ({Build})");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Copiado", "Versão copiada para a área de transferência.", "OK"));
            });
        }

        private static async Task OpenWebsiteAsync()
        {
            const string url = "https://www.escolainfosys.somee.com";
            var uri = new Uri(url);

            try
            {
                // Tenta abrir sem lançar exceção
                if (await Launcher.Default.CanOpenAsync(uri))
                {
                    await Launcher.Default.OpenAsync(uri);
                    return;
                }
            }
            catch
            {
                // Ignora: vamos para o fallback
            }

            // Fallback: copia o link e avisa o utilizador
            await Clipboard.SetTextAsync(url);
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Application.Current!.MainPage!.DisplayAlert(
                    "Não suportado",
                    "Não consegui abrir o navegador neste dispositivo. O link foi copiado para a área de transferência.",
                    "OK"));
        }
    }
}

