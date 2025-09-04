using AppSchoolMaui.Services;

namespace AppSchoolMaui.ViewModels
{
    public sealed class AbsencesVm : BaseViewModel
    {
        private readonly ApiService _api;
        public AbsencesVm(ApiService api) => _api = api;

        public List<ApiService.AbsenceItemDto> Items
        {
            get => _items;
            set => Set(ref _items, value);
        }
        private List<ApiService.AbsenceItemDto> _items = new();

        public async Task LoadAsync()
        {
            var resp = await _api.GetAbsencesAsync();   // retorna AbsenceResponse
            Items = (resp?.items ?? Enumerable.Empty<ApiService.AbsenceItemDto>())
                    .OrderByDescending(a => a.Date)
                    .ToList();
        }
    }
}
