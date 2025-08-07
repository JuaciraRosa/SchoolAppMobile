using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class EditProfilePage : ContentPage
{
    private readonly IApiService _apiService;
    private FileResult _selectedPhoto;

    public EditProfilePage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadCurrentProfileAsync();
    }

    private async void LoadCurrentProfileAsync()
    {
        var profile = await _apiService.GetAsync<StudentDto>("students/profile");
        if (profile == null) return;

        UsernameEntry.Text = profile.UserName;

        if (!string.IsNullOrEmpty(profile.ProfilePhoto))
        {
            ProfileImage.Source = ImageSource.FromUri(new Uri(profile.ProfilePhoto));
        }
    }

    private async void OnSelectPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            _selectedPhoto = await MediaPicker.PickPhotoAsync();
            if (_selectedPhoto != null)
            {
                ProfileImage.Source = ImageSource.FromFile(_selectedPhoto.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load photo: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var newUsername = UsernameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(newUsername))
        {
            await DisplayAlert("Validation", "Username cannot be empty", "OK");
            return;
        }

        var success = await _apiService.UpdateStudentProfileAsync(newUsername, _selectedPhoto);

        if (success)
        {
            await DisplayAlert("Success", "Profile updated successfully!", "OK");
            await Shell.Current.GoToAsync(".."); // volta para a página anterior
        }
        else
        {
            await DisplayAlert("Error", "Failed to update profile", "OK");
        }
    }
}