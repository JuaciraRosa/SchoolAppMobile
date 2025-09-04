using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.Services
{
    /// <summary>
    /// Mostra um DisplayAlert dentro do app. Não requer permissões nem código por plataforma.
    /// </summary>
    public sealed class InAppNotifier : IAppNotifications
    {
        public Task ShowAsync(string title, string message)
        {
            // Garante que roda na UI thread
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = Application.Current?.MainPage;
                if (page is not null)
                    await page.DisplayAlert(title ?? "Aviso", message ?? string.Empty, "OK");
            });
        }
    }
}
