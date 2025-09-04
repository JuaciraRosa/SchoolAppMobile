using AppSchoolMaui.ViewModels;

namespace AppSchoolMaui.Pages;

public partial class AbsencesPage : ContentPage
{
    private readonly AbsencesVm _vm;

    public AbsencesPage(AbsencesVm vm)
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