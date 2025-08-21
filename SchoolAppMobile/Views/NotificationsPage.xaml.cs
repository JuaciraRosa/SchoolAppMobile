using SchoolAppMobile.Models;
using SchoolAppMobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SchoolAppMobile.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly IApiService _apiService;

    public ObservableCollection<NotificationDto> Notifications { get; set; } = new();
    public ICommand RefreshCommand { get; }
    public bool IsRefreshing { get; set; }

    public NotificationsPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();

        BindingContext = this;

        RefreshCommand = new Command(async () => await LoadNotificationsAsync());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        try
        {
            IsRefreshing = true;

            var notes = await _apiService.GetListAsync<NotificationDto>("students/notifications")
                        ?? new List<NotificationDto>();

            Notifications.Clear();
            foreach (var n in notes)
                Notifications.Add(n);

            NotificationsListView.ItemsSource = Notifications;

            ExclusionBanner.IsVisible = notes.Any(n =>
                string.Equals(n.Type, "Exclusion", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load notifications: {ex.Message}", "OK");
        }
        finally
        {
            IsRefreshing = false;
            NotificationsRefreshView.IsRefreshing = false;
        }
    }

    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///HomePage");
    }
}