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

            try
            {
                var me = await _api.GetProfileAsync(); // protegido
                Hello = $"Hello, {me.FullName}";

                var feed = await _api.GetFeedAsync(DateTime.UtcNow.AddDays(-30)); // protegido
                Items = feed.items?.ToList() ?? new();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // permanece como convidado
            }
        }
    }
}
