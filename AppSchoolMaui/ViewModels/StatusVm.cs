using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.ViewModels
{
    public sealed class StatusVm : BaseViewModel
    {
        private readonly ApiService _api;
        public StatusVm(ApiService api) => _api = api;

        public List<ApiService.StatusDto> Items { get => _items; set => Set(ref _items, value); }
        List<ApiService.StatusDto> _items = new();

        // ViewModels/StatusVm.cs
        public async Task LoadAsync()
        {
            var data = await _api.GetStatusAsync();
            Items = (data ?? Enumerable.Empty<ApiService.StatusDto>())
                    .OrderBy(s => s.Subject)
                    .ToList();
        }

    }

}
