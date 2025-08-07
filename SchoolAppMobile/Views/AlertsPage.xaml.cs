using SchoolAppMobile.Models;
using SchoolAppMobile.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SchoolAppMobile.Views;

public partial class AlertsPage : ContentPage
{
    private readonly IApiService _apiService;

    public AlertsPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAlertsAsync();
    }

    private async Task LoadAlertsAsync()
    {
        try
        {
            var token = Preferences.Get("jwt_token", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "Token not found", "OK");
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var endpoint = role switch
            {
                "StaffMember" => "alerts/my",
                "Administrator" => "alerts",
                _ => null
            };

            if (endpoint == null)
            {
                await DisplayAlert("Error", "Unauthorized role", "OK");
                return;
            }

            var alerts = await _apiService.GetListAsync<AlertDto>(endpoint);
            AlertsCollection.ItemsSource = alerts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load alerts: " + ex.Message, "OK");
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }

}