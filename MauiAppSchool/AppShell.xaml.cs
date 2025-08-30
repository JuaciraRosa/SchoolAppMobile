namespace MauiAppSchool
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Pages.PublicPage), typeof(Pages.PublicPage));
            Routing.RegisterRoute(nameof(Pages.ChangePhotoPage), typeof(Pages.ChangePhotoPage));
        }
    }
}
