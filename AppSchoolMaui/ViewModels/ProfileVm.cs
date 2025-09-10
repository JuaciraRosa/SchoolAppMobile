using AppSchoolMaui.Pages;
using AppSchoolMaui.Services;
using SkiaSharp;
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

            FullName = me.FullName;
            Email = me.Email;
            Role = me.Role;

            // pode vir null/"" do servidor — deixa o ResolvePhotoUriAsync decidir o default
            PhotoUrlRaw = string.IsNullOrWhiteSpace(me.ProfilePhoto) ? null : me.ProfilePhoto;

            var uri = await _api.ResolvePhotoUriAsync(PhotoUrlRaw);
            Photo = uri is null
                ? null
                : ImageSource.FromUri(new Uri($"{uri}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"));
        }


        async Task SaveAsync()
        {
            if (IsBusy) return; IsBusy = true;
            try
            {
             
                var updated = await _api.UpdateProfileAsync(FullName, null);

                // Reflete o que ficou salvo no servidor
                PhotoUrlRaw = updated.ProfilePhoto;
                var uri = await _api.ResolvePhotoUriAsync(updated.ProfilePhoto);
                if (uri is not null) Photo = ImageSource.FromUri(uri);

                await _notify.ShowAsync("Perfil", "Dados atualizados");
            }
            catch (Exception ex) { await _notify.ShowAsync("Erro", ex.Message); }
            finally { IsBusy = false; }
        }


        // Reduz e exporta SEMPRE como JPEG
        static byte[] DownscaleJpeg(byte[] input, int maxSide = 640, int quality = 80)
        {
            using var bmp = SKBitmap.Decode(input);
            if (bmp is null) return input; // fallback

            var scale = Math.Min((float)maxSide / bmp.Width, (float)maxSide / bmp.Height);
            if (scale > 1f) scale = 1f;

            int w = (int)(bmp.Width * scale);
            int h = (int)(bmp.Height * scale);

            using var resized = scale < 1f ? bmp.Resize(new SKImageInfo(w, h), SKFilterQuality.Medium) : bmp;
            using var img = SKImage.FromBitmap(resized ?? bmp);
            using var data = img.Encode(SKEncodedImageFormat.Jpeg, quality);
            return data.ToArray();
        }

        async Task PickAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Choose a picture",
                    FileTypes = FilePickerFileType.Images
                });
                if (file is null) return;

                await using var s = await file.OpenReadAsync();
                using var ms = new MemoryStream();
                await s.CopyToAsync(ms);
                var original = ms.ToArray();

                // 1) Converte SEMPRE para JPEG (rápido e menor)
                var jpeg = DownscaleJpeg(original, maxSide: 640, quality: 80);

                // 2) Prévia imediata
                Photo = ImageSource.FromStream(() => new MemoryStream(jpeg));

                // 3) Nome do upload FORÇANDO .jpg (evita PNG)
                var uploadName = Path.ChangeExtension(file.FileName, ".jpg");

                // 4) Envia (ApiService usa o MIME pelo nome, então virá image/jpeg)
                var up = await _api.UploadStudentProfilePhotoAsync(jpeg, uploadName);

                // 5) Guarda só o path retornado (o site espera filename em /uploads)
                PhotoUrlRaw = up.path;

                // 6) Cache-busting usando domínio do site
                var uri = await _api.ResolvePhotoUriAsync(up.path);
                if (uri != null)
                {
                    var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var fresh = new Uri(uri + (uri.Query.Length == 0 ? $"?t={ts}" : $"&t={ts}"));
                    Photo = ImageSource.FromUri(fresh);
                }

                await _notify.ShowAsync("Perfil", "Foto atualizada no servidor.");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await _notify.ShowAsync("Perfil", ex.Message);
            }
        }


    }
}

