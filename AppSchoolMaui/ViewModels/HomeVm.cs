using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.ViewModels
{
    public sealed class HomeVm : BaseViewModel
    {
        private readonly ApiService _api;
        public HomeVm(ApiService api) => _api = api;

        public string Hello { get => _hello; set => Set(ref _hello, value); }
        string _hello = "Olá";

        public List<ApiService.FeedItem> Items { get => _items; set => Set(ref _items, value); }
        List<ApiService.FeedItem> _items = new();
        public async Task LoadAsync()
        {
            Hello = "Hello, guest";
            Items = new();

            // se não há token OU está em modo convidado, não chama endpoints protegidos
            var isGuest = Preferences.Get("guest", false) || !_api.HasAuth;
            if (isGuest) return;

            try
            {
                var me = await _api.GetProfileAsync();           // protegido
                Hello = $"Hello, {me.FullName}";

                var feed = await _api.GetFeedAsync(DateTime.UtcNow.AddDays(-30)); // protegido
                Items = feed.items?.ToList() ?? new();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // token inválido/expirado → regressa a guest silenciosamente
                await _api.LogoutAsync();
                Preferences.Set("guest", true);
            }
        }

    }
}
