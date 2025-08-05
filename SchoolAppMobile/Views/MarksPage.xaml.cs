using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class MarksPage : ContentPage
{
    private readonly IApiService _apiService;

    public MarksPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadMarksAsync();
    }

    private async void LoadMarksAsync()
    {
        var marks = await _apiService.GetListAsync<MarkDto>("students/marks");
        MarksCollection.ItemsSource = marks;
    }
}