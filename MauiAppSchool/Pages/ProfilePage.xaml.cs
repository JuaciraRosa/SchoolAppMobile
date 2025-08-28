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


    // Toque na foto → escolher URL ou pré-visualizar do dispositivo
    // Toque na foto → escolher URL ou pré-visualização do dispositivo
    private async void OnChangePhoto(object s, EventArgs e)
    {
        var choice = await DisplayActionSheet("Change photo", "Cancel", null,
                                              "Paste URL (site)", "Pick from device (preview)");

        if (choice == "Paste URL (site)")
        {
            var url = await DisplayPromptAsync("Photo URL", "Paste an absolute or site-relative URL");
            if (string.IsNullOrWhiteSpace(url)) return;

            PhotoUrl.Text = url;
            try
            {
                var uri = await _api.ResolvePhotoUriAsync(url);
                Photo.Source = uri is null ? null : ImageSource.FromUri(uri);
            }
            catch { /* ignore */ }

        }
        else if (choice == "Pick from device (preview)")
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Choose a picture",
                    FileTypes = FilePickerFileType.Images
                });
                if (file is null) return;

                using var stream = await file.OpenReadAsync();

                // Bufferiza para poder reusar o stream
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                // Mostra a preview local
                Photo.Source = ImageSource.FromStream(() => new MemoryStream(bytes));

                // IMPORTANTE:
                // A API hoje espera uma string/URL do site. Sem endpoint de upload,
                // não dá para "salvar" esta foto local no servidor.
                // Quando tiver upload, aqui você:
                //  - faz upload -> recebe 'path'
                //  - PhotoUrl.Text = path;
                //  - await _api.UpdateProfileAsync(profilePhoto: path);
                //  - await Load();
            }
            catch { /* cancelado */ }
        }

    }



    private async void GoHome(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//home");
}