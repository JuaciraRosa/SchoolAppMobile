namespace AppSchoolMaui.Pages;


public partial class EnrollmentRequestsPage : ContentPage
{
    private readonly AppSchoolMaui.ViewModels.EnrollmentRequestsVm _vm;

    public EnrollmentRequestsPage(AppSchoolMaui.ViewModels.EnrollmentRequestsVm vm)
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