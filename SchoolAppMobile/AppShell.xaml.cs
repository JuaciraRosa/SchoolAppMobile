namespace SchoolAppMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SchoolAppMobile.Views.ResetPasswordPage),
                      typeof(SchoolAppMobile.Views.ResetPasswordPage));

        }

      

    }
}
