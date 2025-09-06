using AppSchoolMaui.Pages;
using AppSchoolMaui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.ViewModels
{
    public sealed class ProfileVm : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly IAppNotifications _notify;

        public ProfileVm(ApiService api, IAppNotifications notify)
        {
            _api = api; _notify = notify;
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
            PickPhotoCommand = new Command(async () => await PickAsync(), () => !IsBusy);
            ChangePasswordCommand = new Command(async () => await _api.ChangePasswordOnWebAsync());
            OpenEnrollmentRequestsCommand = new Command(async () =>
                await Shell.Current.GoToAsync(nameof(EnrollmentRequestsPage)));

        }
        public Command ChangePasswordCommand { get; }
        public Command OpenEnrollmentRequestsCommand { get; }

        public Command SaveCommand { get; }
        public Command PickPhotoCommand { get; }

        bool _isBusy; public bool IsBusy { get => _isBusy; set { Set(ref _isBusy, value); SaveCommand.ChangeCanExecute(); PickPhotoCommand.ChangeCanExecute(); } }

        public string? FullName { get => _full; set => Set(ref _full, value); }
        string? _full;
        public string? Email { get => _email; set => Set(ref _email, value); }
        string? _email;
        public string? Role { get => _role; set => Set(ref _role, value); }
        string? _role;
        public ImageSource? Photo { get => _photo; set => Set(ref _photo, value); }
        ImageSource? _photo;
        public string? PhotoUrlRaw { get => _url; set => Set(ref _url, value); }
        string? _url;

        public async Task LoadAsync()
        {
            var me = await _api.GetProfileAsync();
            FullName = me.FullName; Email = me.Email; Role = me.Role; PhotoUrlRaw = me.ProfilePhoto;

            try
            {
                var uri = await _api.ResolvePhotoUriAsync(me.ProfilePhoto);
                Photo = uri is null ? null : ImageSource.FromUri(uri);
            }
            catch { Photo = null; }
        }

        async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true;
            try
            {
                // grava nome e/ou caminho da foto
                var updated = await _api.UpdateProfileAsync(FullName, PhotoUrlRaw);

                // reflete o que ficou salvo no servidor
                PhotoUrlRaw = updated.ProfilePhoto;
                var uri = await _api.ResolvePhotoUriAsync(updated.ProfilePhoto);
                if (uri is not null) Photo = ImageSource.FromUri(uri);

                await _notify.ShowAsync("Perfil", "Dados atualizados");
            }
            catch (Exception ex)
            {
                await _notify.ShowAsync("Erro", ex.Message);
            }
            finally { IsBusy = false; }
        }


        async Task PickAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(
                    new PickOptions { PickerTitle = "Choose a picture", FileTypes = FilePickerFileType.Images });
                if (file is null) return;

                await using var s = await file.OpenReadAsync();
                using var ms = new MemoryStream(); await s.CopyToAsync(ms);
                var bytes = ms.ToArray();

             
                var up = await _api.UploadStudentProfilePhotoAsync(bytes, file.FileName);
                PhotoUrlRaw = up.url;

          
                var uri = new Uri(up.url + (up.url.Contains('?') ? "&" : "?") + "t=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                Photo = ImageSource.FromUri(uri);

                await _notify.ShowAsync("Perfil", "Foto atualizada");
            }
            catch (OperationCanceledException) { }
        }

    }
}

