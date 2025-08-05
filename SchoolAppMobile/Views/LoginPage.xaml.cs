using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService = new();

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ErrorLabel.Text = "Please enter email and password.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var token = await _apiService.LoginAsync(email, password);

        if (string.IsNullOrEmpty(token))
        {
            ErrorLabel.Text = "Login failed. Check your credentials.";
            ErrorLabel.IsVisible = true;
            return;
        }

        // Salvar o token para uso futuro (em Preferences por exemplo)
        Preferences.Set("auth_token", token);

        // Navegar para HomePage (criaremos em seguida)
        await Shell.Current.GoToAsync("//home");
    }
}