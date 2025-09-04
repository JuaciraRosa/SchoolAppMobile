using AppSchoolMaui.ViewModels;

namespace AppSchoolMaui.Pages;

public partial class PublicPage : ContentPage
{
    private readonly PublicVm _vm;

    public PublicPage(PublicVm vm)
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