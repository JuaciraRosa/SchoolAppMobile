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
        var averages = await _apiService.GetListAsync<AverageDto>("staff/averages");
        AveragesListView.ItemsSource = averages;
    }

    
}
