using AppSchoolMaui.ViewModels;

namespace AppSchoolMaui.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileVm _vm;

    public ProfilePage(ProfileVm vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}