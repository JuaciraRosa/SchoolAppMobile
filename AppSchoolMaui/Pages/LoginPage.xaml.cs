using AppSchoolMaui.ViewModels;


namespace AppSchoolMaui.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

  

}