using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly IApiService _apiService;

    public ProfilePage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadProfileAsync();
    }

    private async void LoadProfileAsync()
    {
        var profile = await _apiService.GetAsync<StudentDto>("students/profile");
        if (profile == null) return;

        NameLabel.Text = $"Username: {profile.UserName}";
        EmailLabel.Text = $"Email: {profile.Email}";
        CourseLabel.Text = $"Course: {profile.Course}";

        if (!string.IsNullOrEmpty(profile.ProfilePhoto))
        {
            // Garante que seja uma URL completa
            if (!profile.ProfilePhoto.StartsWith("http"))
            {
                profile.ProfilePhoto = "https://www.escolainfosysapi.somee.com" + profile.ProfilePhoto;
            }

            ProfileImage.Source = ImageSource.FromUri(new Uri(profile.ProfilePhoto));
        }
    }


    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditProfilePage());
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }
}