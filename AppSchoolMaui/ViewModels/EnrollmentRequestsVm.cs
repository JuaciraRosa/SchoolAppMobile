using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppSchoolMaui.ViewModels
{
    public sealed class EnrollmentRequestsVm : BaseViewModel
    {
        private readonly ApiService _api;
        public EnrollmentRequestsVm(ApiService api)
        {
            _api = api;
            RefreshCommand = new Command(async () => await LoadAsync());
            SendCommand = new Command(async () => await SendAsync(), () => Selected is not null && !IsBusy);
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { Set(ref _isBusy, value); SendCommand.ChangeCanExecute(); } }

        // Banner de simulação
        private bool _isSimulated;
        public bool IsSimulated { get => _isSimulated; set => Set(ref _isSimulated, value); }

        public ObservableCollection<ApiService.EnrollmentRequestDto> Requests { get; } = new();
        public ObservableCollection<ApiService.SubjectDto> Subjects { get; } = new();

        private ApiService.SubjectDto? _selected;
        public ApiService.SubjectDto? Selected
        {
            get => _selected;
            set { Set(ref _selected, value); SendCommand.ChangeCanExecute(); }
        }

        private string? _note;
        public string? Note { get => _note; set => Set(ref _note, value); }

        public Command RefreshCommand { get; }
        public Command SendCommand { get; }

        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                Subjects.Clear();
                foreach (var s in await _api.GetPublicSubjectsAsync() ?? new()) Subjects.Add(s);

                IsSimulated = false;
                Requests.Clear();
                foreach (var r in await _api.GetMyEnrollmentRequestsAsync() ?? new()) Requests.Add(r);
            }
            catch (HttpRequestException ex)
            {
                // 500 do servidor ou mensagem do DbContext => MODO SIMULADO
                if (ex.StatusCode == HttpStatusCode.InternalServerError
                    || (ex.Message?.IndexOf("DbContext", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    IsSimulated = true;
                    Requests.Clear();
                    foreach (var r in await EnrollmentRequestLocalStore.LoadAsync()) Requests.Add(r);
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
                }
            }
            finally { IsBusy = false; }
        }

        private async Task SendAsync()
        {
            if (Selected is null) return;
            try
            {
                IsBusy = true;
                if (IsSimulated)
                    await EnrollmentRequestLocalStore.AddAsync(Selected.Id, Selected.Name, Note);
                else
                    await _api.CreateEnrollmentRequestAsync(Selected.Id, Note);

                Note = null; Selected = null;
                await LoadAsync();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.InternalServerError
                    || (ex.Message?.IndexOf("DbContext", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    IsSimulated = true;
                    await EnrollmentRequestLocalStore.AddAsync(Selected.Id, Selected.Name, Note);
                    Note = null; Selected = null;
                    await LoadAsync();
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
                }
            }
            finally { IsBusy = false; }
        }

    }
}