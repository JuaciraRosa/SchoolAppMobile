using AppSchoolMaui.ViewModels;

namespace AppSchoolMaui.Pages;

public partial class MarksPage : ContentPage
{
    private readonly MarksVm _vm;

    public MarksPage(MarksVm vm)
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
