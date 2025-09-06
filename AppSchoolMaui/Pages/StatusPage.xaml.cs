using AppSchoolMaui.Helpers;
using AppSchoolMaui.ViewModels;
using System.Net;

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
}