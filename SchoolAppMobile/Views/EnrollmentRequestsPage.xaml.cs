using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class EnrollmentRequestsPage : ContentPage
{
    private readonly IApiService _apiService;
    private List<EnrollmentRequestDto> _requests;

    public EnrollmentRequestsPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadRequests();
    }

    private async void LoadRequests()
    {
        _requests = await _apiService.GetListAsync<EnrollmentRequestDto>("admin/enrollmentrequests");
        RequestsCollection.ItemsSource = _requests;
    }

    private async void OnApproveClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is EnrollmentRequestDto request)
        {
            var success = await _apiService.PostAsync<object>($"admin/enrollmentrequests/{request.Id}/approve", null);
            if (success)
            {
                await DisplayAlert("Success", "Request approved.", "OK");
                LoadRequests();
            }
        }
    }

    private async void OnRejectClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is EnrollmentRequestDto request)
        {
            var success = await _apiService.PostAsync<object>($"admin/enrollmentrequests/{request.Id}/reject", null);
            if (success)
            {
                await DisplayAlert("Rejected", "Request rejected.", "OK");
                LoadRequests();
            }
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }

}
