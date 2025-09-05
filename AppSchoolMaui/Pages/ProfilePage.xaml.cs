using AppSchoolMaui.Services;
using AppSchoolMaui.ViewModels;

namespace AppSchoolMaui.Pages;
public partial class ProfilePage : ContentPage
{
    private readonly ProfileVm _vm;
    private readonly ApiService _api;



    public ProfilePage(ProfileVm vm, ApiService api)
    {
        InitializeComponent();
        _vm = vm;             // <-- FALTAVA isto
        _api = api;

        BindingContext = _vm;  // <-- usa o VM injetado; não uses ServiceHelper aqui
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    // Só mantém estes handlers se o XAML usar Clicked=. Se usares Command=, podes apagar ambos.
    private async void OnChangePassword(object s, EventArgs e)
        => await _api.ChangePasswordOnWebAsync();

    private async void OnOpenEnrollments(object s, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(EnrollmentRequestsPage));

    private async void OnOpenAbout(object sender, EventArgs e)
    => await Shell.Current.GoToAsync(nameof(AboutPage));

    private async void OnOpenChangePassword(object s, EventArgs e)
    => await Shell.Current.GoToAsync(nameof(ChangePasswordPage));


}