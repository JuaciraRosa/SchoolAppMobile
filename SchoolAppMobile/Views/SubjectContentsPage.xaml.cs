using SchoolAppMobile.Services;

namespace SchoolAppMobile.Views;

[QueryProperty(nameof(SubjectId), "subjectId")]
[QueryProperty(nameof(SubjectName), "subjectName")]
public partial class SubjectContentsPage : ContentPage
{
    private readonly IApiService _api;

    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "";

    public SubjectContentsPage()
    {
        InitializeComponent();
        _api = ServiceHelper.GetService<IApiService>();
        Refresh.Command = new Command(async () => await LoadAsync());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Header.Text = $"Contents · {SubjectName}";
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            Loading.IsVisible = Loading.IsRunning = true;
            EmptyLabel.IsVisible = false;

            var list = await _api.GetPublicContentsBySubjectAsync(SubjectId);
            ContentsList.ItemsSource = list;
            EmptyLabel.IsVisible = list == null || list.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load contents: {ex.Message}", "OK");
            EmptyLabel.IsVisible = true;
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
            Refresh.IsRefreshing = false;
        }
    }
}