using SchoolAppMobile.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SchoolAppMobile.Views;

public partial class HomePage : ContentPage
{
    private readonly IApiService _apiService;

    public HomePage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadUIAsync();
    }

    private async void LoadUIAsync()
    {
        var token = Preferences.Get("jwt_token", string.Empty);
        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("///LoginPage");
            return;
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        WelcomeLabel.Text = $"Welcome, {role}";

        ButtonsPanel.Children.Clear();

        switch (role)
        {
            case "Student":
                AddButton("Marks", async () => await Shell.Current.GoToAsync("///MarksPage"));
                AddButton("Absences", async () => await Shell.Current.GoToAsync("///AbsencesPage"));
                AddButton("Notifications", async () => await Shell.Current.GoToAsync("///NotificationsPage"));
                AddButton("Profile", async () => await Shell.Current.GoToAsync("///ProfilePage"));
                AddButton("Enrollment Request", async () => await Shell.Current.GoToAsync("///SendEnrollmentPage"));
                AddButton("Explore Courses", async () => await Shell.Current.GoToAsync("///ExplorePage"));
                break;

            case "StaffMember":
                AddButton("My Students", async () => await Shell.Current.GoToAsync("///StaffStudentsPage"));
                AddButton("Register Mark", async () => await Shell.Current.GoToAsync("///RegisterMarkPage"));
                AddButton("Averages", async () => await Shell.Current.GoToAsync("///AveragesPage"));
                AddButton("Alerts", async () => await Shell.Current.GoToAsync("///AlertsPage"));
              
                AddButton("Explore Courses", async () => await Shell.Current.GoToAsync("///ExplorePage"));
                break;

            case "Administrator":
                AddButton("Enrollment Requests", async () => await Shell.Current.GoToAsync("///EnrollmentRequestsPage"));
                AddButton("System Settings", async () => await Shell.Current.GoToAsync("///SettingsPage"));
                AddButton("Alerts", async () => await Shell.Current.GoToAsync("///AlertsPage"));
          
                AddButton("Explore Courses", async () => await Shell.Current.GoToAsync("///ExplorePage"));
                break;

     
            default:
                AddButton("Explore Courses", async () => await Shell.Current.GoToAsync("///ExplorePage"));
                AddButton("Login", async () => await Shell.Current.GoToAsync("///LoginPage"));
                break;
        }

    }

    private void AddButton(string text, Func<Task> action)
    {
        var btn = new Button
        {
            Text = text,
            BackgroundColor = Colors.DarkSlateBlue,
            TextColor = Colors.White
        };
        btn.Clicked += async (s, e) => await action();
        ButtonsPanel.Children.Add(btn);
    }

    private async void OnCreditsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///CreditsPage");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Remove("jwt_token");
        await Shell.Current.GoToAsync("///LoginPage");
    }
}