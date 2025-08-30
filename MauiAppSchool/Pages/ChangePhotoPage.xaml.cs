using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class ChangePhotoPage : ContentPage
{
    private readonly ApiService _api;
    private byte[]? _bytes;
    private string? _fileName;

    public ChangePhotoPage() : this(ServiceHelper.GetRequiredService<ApiService>()) { }
    public ChangePhotoPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }
    private async void OnPick(object sender, EventArgs e)
    {
        Msg.IsVisible = Err.IsVisible = false;

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
            _bytes = ms.ToArray();
            _fileName = file.FileName;

            Preview.Source = ImageSource.FromStream(() => new MemoryStream(_bytes));
            SaveButton.IsEnabled = true; // <- aqui
        }
        catch (Exception ex)
        {
            Err.Text = ex.Message;
            Err.IsVisible = true;
        }
    }


    private async void OnSave(object sender, EventArgs e)
    {
        Msg.IsVisible = Err.IsVisible = false;

        try
        {
            if (_bytes is null || string.IsNullOrWhiteSpace(_fileName))
            {
                Err.Text = "Pick a photo first.";
                Err.IsVisible = true;
                return;
            }

            var up = await _api.UploadStudentProfilePhotoAsync(_bytes, _fileName);

            // opcional: atualiza o campo ProfilePhoto para o path relativo devolvido
            await _api.UpdateProfileAsync(null, up.path);

            Msg.Text = "Photo updated.";
            Msg.IsVisible = true;

            // volta para o perfil
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Err.Text = ex.Message;
            Err.IsVisible = true;
        }
    }

    private async void OpenChangePhotoInApp(object s, EventArgs e)
    {
        await Navigation.PushAsync(new Pages.ChangePhotoPage());
    }

}