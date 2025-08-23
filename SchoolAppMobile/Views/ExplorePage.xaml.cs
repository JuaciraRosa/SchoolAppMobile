using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

public partial class ExplorePage : ContentPage
{
    private readonly IApiService _api;
    private CancellationTokenSource? _cts;

    public ExplorePage()
    {
        InitializeComponent();
        _api = ServiceHelper.GetService<IApiService>();
        Refresh.Command = new Command(async () => await LoadAsync());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            Loading.IsVisible = Loading.IsRunning = true;
            EmptyLabel.IsVisible = false;

            var list = await _api.GetPublicCoursesAsync();
            CoursesList.ItemsSource = list;
            EmptyLabel.IsVisible = list == null || list.Count == 0;
        }
        catch (TaskCanceledException) { /* ignore */ }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load courses: {ex.Message}", "OK");
            EmptyLabel.IsVisible = true;
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
            Refresh.IsRefreshing = false;
        }
    }

    private async void OnCourseSelected(object sender, SelectionChangedEventArgs e)
    {
        var course = e.CurrentSelection?.FirstOrDefault() as CourseDto;
        ((CollectionView)sender).SelectedItem = null;
        if (course == null) return;

        await Shell.Current.GoToAsync($"{nameof(PublicSubjectsPage)}?courseId={course.Id}&courseName={Uri.EscapeDataString(course.Name)}");
    }
}