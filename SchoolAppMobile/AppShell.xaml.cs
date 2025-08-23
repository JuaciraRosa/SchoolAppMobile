using SchoolAppMobile.Views;

namespace SchoolAppMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SchoolAppMobile.Views.ResetPasswordPage),
                      typeof(SchoolAppMobile.Views.ResetPasswordPage));
            Routing.RegisterRoute(nameof(PublicSubjectsPage), typeof(PublicSubjectsPage));
            Routing.RegisterRoute(nameof(SubjectContentsPage), typeof(SubjectContentsPage));


        }

      

      

    }
}
