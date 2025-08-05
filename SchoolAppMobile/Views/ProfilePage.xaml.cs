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
        var profile = await _apiService.GetAsync<ProfileDto>("students/profile");
        if (profile == null) return;

        NameLabel.Text = $"Username: {profile.UserName}";
        EmailLabel.Text = $"Email: {profile.Email}";
        CourseLabel.Text = $"Course: {profile.Course}";

        if (!string.IsNullOrEmpty(profile.ProfilePhoto))
        {
            ProfilePhoto.Source = ImageSource.FromUri(new Uri(profile.ProfilePhoto));
        }
    }
}