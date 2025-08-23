using SchoolAppMobile.Models;
using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

[QueryProperty(nameof(CourseId), "courseId")]
[QueryProperty(nameof(CourseName), "courseName")]
public partial class PublicSubjectsPage : ContentPage
{
    private readonly IApiService _api;
    private CancellationTokenSource? _cts;

    public int CourseId { get; set; }
    public string CourseName { get; set; } = "";

    public PublicSubjectsPage()
    {
        InitializeComponent();
        _api = ServiceHelper.GetService<IApiService>();
        Refresh.Command = new Command(async () => await LoadAsync());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Header.Text = $"Subjects in {CourseName}";
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

            var list = await _api.GetPublicSubjectsByCourseAsync(CourseId);
            SubjectsList.ItemsSource = list;
            EmptyLabel.IsVisible = list == null || list.Count == 0;
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load subjects: {ex.Message}", "OK");
            EmptyLabel.IsVisible = true;
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
            Refresh.IsRefreshing = false;
        }
    }

    private async void OnSubjectSelected(object sender, SelectionChangedEventArgs e)
    {
        var subj = e.CurrentSelection?.FirstOrDefault() as SubjectDto;
        ((CollectionView)sender).SelectedItem = null;
        if (subj == null) return;

        await Shell.Current.GoToAsync($"{nameof(SubjectContentsPage)}?subjectId={subj.Id}&subjectName={Uri.EscapeDataString(subj.Name)}");
    }
}