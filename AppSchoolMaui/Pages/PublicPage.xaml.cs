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
        try { await _vm.LoadAsync(); }
        catch (Exception ex) { await DisplayAlert("Erro", ex.Message, "OK"); }
    }
}
