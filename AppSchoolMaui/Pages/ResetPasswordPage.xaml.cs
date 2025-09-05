namespace AppSchoolMaui.Pages;

public partial class ResetPasswordPage : ContentPage
{
    public ResetPasswordPage(AppSchoolMaui.ViewModels.ResetPasswordVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    void OnShowPwChanged(object sender, CheckedChangedEventArgs e)
    {
        var mask = !e.Value;
        if (New1Entry != null) New1Entry.IsPassword = mask;
        if (New2Entry != null) New2Entry.IsPassword = mask;
    }
}