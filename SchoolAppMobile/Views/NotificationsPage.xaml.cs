using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly IApiService _apiService;

    public NotificationsPage()
    {
        InitializeComponent();
        _apiService = ServiceHelper.GetService<IApiService>();
        LoadNotificationsAsync();
    }

    private async void LoadNotificationsAsync()
    {
        var notifications = await _apiService.GetListAsync<NotificationDto>("students/notifications");
        NotificationsListView.ItemsSource = notifications;
    }
}