using AppSchoolMaui.ViewModels;

namespace AppSchoolMaui.Pages;

public partial class StatusPage : ContentPage
{
    private readonly StatusVm _vm;

    
    public StatusPage(StatusVm vm)
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