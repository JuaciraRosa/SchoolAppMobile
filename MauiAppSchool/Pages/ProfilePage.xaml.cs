using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;
    private ApiService.ProfileVm? _current; // perfil carregado

    // Construtor usado pelo Shell/DI
    public ProfilePage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }

    // Construtor principal (DI resolve este)
    public ProfilePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }

    // Sobrecarga opcional (se quiser criar manualmente passando o perfil)
    public ProfilePage(ApiService api, ApiService.ProfileVm? current) : this(api)
    {
        _current = current;
    }
    private async Task Load()
    {
        var me = await _api.GetProfileAsync();
        _current = me;

        FullName.Text = me.FullName;
        PhotoUrl.Text = me.ProfilePhoto;
        Email.Text = $"Email: {me.Email}";
        Role.Text = $"Role: {me.Role}";

        try
        {
            var uri = await _api.ResolvePhotoUriAsync(me.ProfilePhoto);
            Photo.Source = uri is null ? null : ImageSource.FromUri(uri);
        }
        catch { Photo.Source = null; }
    }

    private async void OnSave(object s, EventArgs e)
    {
        try
        {
            var fullName = FullName.Text?.Trim();
            var photo = PhotoUrl.Text?.Trim();

            // Só envia o que realmente mudou
            string? sendName = (_current is null || !string.Equals(_current.FullName, fullName, StringComparison.Ordinal))
                                ? fullName : null;
            string? sendPhoto = (_current is null || !string.Equals(_current.ProfilePhoto ?? "", photo ?? "", StringComparison.Ordinal))
                                ? photo : null;

            await _api.UpdateProfileAsync(sendName, sendPhoto);
            await Load();
            await DisplayAlert("Saved", "Profile updated.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnChangePhoto(object s, EventArgs e)
    {
        try
        {
            var file = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Choose a picture",
                FileTypes = FilePickerFileType.Images
            });
            if (file is null) return;

            // PREVIEW imediato
            await using var stream = await file.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            Photo.Source = ImageSource.FromStream(() => new MemoryStream(bytes));

            // UPLOAD real para o API (POST /api/students/profile/photo)
            var up = await _api.UploadStudentProfilePhotoAsync(bytes, file.FileName);

            // Mostra a URL pública que o API devolveu
            Photo.Source = ImageSource.FromUri(new Uri(up.url));

            // Recarrega para sincronizar nome/foto
            await Load();

            await DisplayAlert("Saved", "Photo updated.", "OK");
        }
        catch (OperationCanceledException) { /* cancelado */ }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }



    private async void GoHome(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//home");

    private async void OpenEditProfileOnWeb(object s, EventArgs e)
    => await Browser.OpenAsync(new Uri($"{_api.WebBase}/Account/EditProfile"),
                               BrowserLaunchMode.External);

    private async void OpenChangePasswordOnWeb(object s, EventArgs e)
        => await Browser.OpenAsync(new Uri($"{_api.WebBase}/Account/ChangePassword"),
                                   BrowserLaunchMode.External);
}