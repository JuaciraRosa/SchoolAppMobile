using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class SettingsPage : ContentPage
{
    private readonly IApiService _apiService;

    public SettingsPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadSettingsAsync();
    }

    private async void LoadSettingsAsync()
    {
        var settings = await _apiService.GetAsync<SystemSettingsDto>("systemsettings");
        if (settings != null)
        {
            await DisplayAlert("Max Absences", $"Max allowed: {settings.MaxAbsencesPercentage}%", "OK");
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }
   


}