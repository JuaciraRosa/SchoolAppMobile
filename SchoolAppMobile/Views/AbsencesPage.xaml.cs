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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAbsencesAsync();
    }

    private async Task LoadAbsencesAsync()
    {
        LoadingIndicator.IsVisible = true;
        AbsencesCollection.IsVisible = false;
        NoDataLabel.IsVisible = false;

        try
        {
            var absences = await _apiService.GetListAsync<AbsenceDto>("students/absences");

            if (absences.Any())
            {
                AbsencesCollection.ItemsSource = absences;
                AbsencesCollection.IsVisible = true;
            }
            else
            {
                NoDataLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load absences: " + ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }

}