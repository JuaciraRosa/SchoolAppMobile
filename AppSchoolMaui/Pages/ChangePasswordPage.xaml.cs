namespace AppSchoolMaui.Pages;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(AppSchoolMaui.ViewModels.ChangePasswordVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // quando o checkbox muda, mostra/oculta o texto
    void OnShowPwChanged(object sender, CheckedChangedEventArgs e)
    {
        var show = e.Value;         // true = mostrar
        var mask = !show;           // IsPassword usa lógica inversa

        if (CurrentEntry != null) CurrentEntry.IsPassword = mask;
        if (New1Entry != null) New1Entry.IsPassword = mask;
        if (New2Entry != null) New2Entry.IsPassword = mask;
    }
}