using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;

    public ProfilePage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public ProfilePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Loaded += async (_, __) => await Load();
    }

    private async Task Load()
    {
        var me = await _api.GetProfileAsync();
        FullName.Text = me.FullName;
        PhotoUrl.Text = me.ProfilePhoto;
        Email.Text = $"Email: {me.Email}";
        Role.Text  = $"Role: {me.Role}";

        try
        {
            var uri = _api.ResolvePhotoUri(me.ProfilePhoto);
            Photo.Source = uri is null ? null : ImageSource.FromUri(uri);
        }
        catch { Photo.Source = null; }
    }

    private async void OnSave(object s, EventArgs e)
    {
        await _api.UpdateProfileAsync(FullName.Text, PhotoUrl.Text);
        await Load();
        await DisplayAlert("Saved", "Profile updated.", "OK");
    }

    // Toque na foto → escolher URL ou pré-visualizar do dispositivo
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
                var uri = _api.ResolvePhotoUri(url);
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

                // DEPOIS (bufferizado)
                using var stream = await file.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                Photo.Source = ImageSource.FromStream(() => new MemoryStream(bytes));



                await DisplayAlert("Note",
                    "This only previews the image. To persist on the server, paste a URL from the site.",
                    "OK");
            }
            catch { /* canceled */ }
        }
    }


    private async void GoHome(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//home");
}