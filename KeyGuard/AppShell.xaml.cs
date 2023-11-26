using KeyGuard.Views;
using KeyGuard.Views.Mobile;

namespace KeyGuard
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(CreateManagerPage), typeof(CreateManagerPage));
            Routing.RegisterRoute(nameof(DecryptPage), typeof(DecryptPage));
            Routing.RegisterRoute(nameof(KeysPage), typeof(KeysPage));
            Routing.RegisterRoute(nameof(AddKeyPage), typeof(AddKeyPage));
            Routing.RegisterRoute(nameof(EditKeyPage), typeof(EditKeyPage));
            Routing.RegisterRoute(nameof(FileOptionsPage), typeof(FileOptionsPage));
            Routing.RegisterRoute(nameof(AboutOptionsPage), typeof(AboutOptionsPage));
        }
    }
}