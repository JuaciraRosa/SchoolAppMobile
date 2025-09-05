namespace AppSchoolMaui.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage(AppSchoolMaui.ViewModels.AboutVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}