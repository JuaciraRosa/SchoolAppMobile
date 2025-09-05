using AppSchoolMaui.ViewModels;
using AppSchoolMaui.ViewModels.AppSchoolMaui.ViewModels;

namespace AppSchoolMaui.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnOpenReset(object sender, EventArgs e)
    => await Shell.Current.GoToAsync(nameof(ResetPasswordPage));

}