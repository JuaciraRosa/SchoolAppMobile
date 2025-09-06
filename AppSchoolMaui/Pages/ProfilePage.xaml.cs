using AppSchoolMaui.Helpers;
using AppSchoolMaui.Services;
using AppSchoolMaui.ViewModels;
using System.Net;

namespace AppSchoolMaui.Pages;
public partial class ProfilePage : ContentPage
{
    private readonly ProfileVm _vm;
    private readonly ApiService _api;



    public ProfilePage(ProfileVm vm, ApiService api)
    {
        InitializeComponent();
        _vm = vm;             
        _api = api;

        BindingContext = _vm;  
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _vm.LoadAsync();
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Aviso", ex.ToUserMessage(), "OK");
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
                await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }


    private async void OnChangePassword(object s, EventArgs e)
    {
        try { await _api.ChangePasswordOnWebAsync(); }
        catch (HttpRequestException ex) { await DisplayAlert("Aviso", ex.ToUserMessage(), "OK"); }
        catch (Exception ex) { await DisplayAlert("Erro", ex.Message, "OK"); }
    }
    private async void OnOpenEnrollments(object s, EventArgs e)
    {
        try { await Shell.Current.GoToAsync(nameof(EnrollmentRequestsPage)); }
        catch (Exception ex) { await DisplayAlert("Erro", ex.Message, "OK"); }
    }

    private async void OnOpenAbout(object sender, EventArgs e)
    {
        try { await Shell.Current.GoToAsync(nameof(AboutPage)); }
        catch (Exception ex) { await DisplayAlert("Erro", ex.Message, "OK"); }
    }

    private async void OnOpenChangePassword(object s, EventArgs e)
    {
        try { await Shell.Current.GoToAsync(nameof(ChangePasswordPage)); }
        catch (Exception ex) { await DisplayAlert("Erro", ex.Message, "OK"); }
    }

}