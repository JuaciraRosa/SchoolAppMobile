using MauiAppSchool.Helpers;
using MauiAppSchool.Services;

namespace MauiAppSchool.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;

    // construtor sem parâmetros (usado pelo XAML/Shell)
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
        Role.Text = $"Role: {me.Role}";

        try
        {
            var uri = _api.ResolvePhotoUri(me.ProfilePhoto);
            Photo.Source = uri is null ? null : ImageSource.FromUri(uri);
        }
        catch
        {
            Photo.Source = null; // fallback opcional (ou uma imagem local)
        }
    }


    private async void OnSave(object s, EventArgs e)
    {
        await _api.UpdateProfileAsync(FullName.Text, PhotoUrl.Text);
        await Load();
    }

    private async void GoHome(object sender, EventArgs e)
  => await Shell.Current.GoToAsync("///home");

}