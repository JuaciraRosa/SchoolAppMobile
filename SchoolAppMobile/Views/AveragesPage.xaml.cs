using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class AveragesPage : ContentPage
{
    private readonly IApiService _apiService;

    public AveragesPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadAverages();
    }

    private async void LoadAverages()
    {
        try
        {
            var averages = await _apiService.GetListAsync<AverageDto>("staff/averages");
            AveragesListView.ItemsSource = averages;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load averages: {ex.Message}", "OK");
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }

}
