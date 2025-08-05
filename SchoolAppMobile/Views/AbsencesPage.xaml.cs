using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class AbsencesPage : ContentPage
{
    private readonly IApiService _apiService;

    public AbsencesPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadAbsencesAsync();
    }

    private async void LoadAbsencesAsync()
    {
        var absences = await _apiService.GetListAsync<AbsenceDto>("students/absences");
        AbsencesCollection.ItemsSource = absences;
    }
}