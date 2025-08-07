using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class ChangePasswordPage : ContentPage
{
    private readonly IApiService _apiService;

    public ChangePasswordPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        var current = CurrentPasswordEntry.Text?.Trim();
        var newPass = NewPasswordEntry.Text?.Trim();
        var confirm = ConfirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirm))
        {
            await DisplayAlert("Validation", "All fields are required", "OK");
            return;
        }

        if (newPass != confirm)
        {
            await DisplayAlert("Validation", "Passwords do not match", "OK");
            return;
        }

        var dto = new ChangePasswordDto
        {
            CurrentPassword = current,
            NewPassword = newPass
        };

        var result = await _apiService.ChangePasswordAsync(dto);

        if (result)
        {
            await DisplayAlert("Success", "Password changed successfully", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Failed to change password", "OK");
        }
    }
}