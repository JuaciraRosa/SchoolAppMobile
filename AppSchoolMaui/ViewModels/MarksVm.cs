using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AppSchoolMaui.Services.ApiService;

namespace AppSchoolMaui.ViewModels
{
    public class MarksVm
    {
        private readonly ApiService _api;
        public ObservableCollection<MarkDto> Items { get; } = new();

        public MarksVm(ApiService api) => _api = api;

        public async Task LoadAsync()
        {
            var list = await _api.GetMarksAsync(); // endpoint real
            Items.Clear();
            foreach (var m in list) Items.Add(m);



        }


    }

}
