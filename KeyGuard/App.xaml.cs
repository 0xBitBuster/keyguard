namespace KeyGuard
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Width = 550;
            window.MinimumWidth = 550;
            window.Height = 750;
            window.MinimumHeight = 750;

            window.Destroying += Window_Destroying;

            return window;
        }

        private void Window_Destroying(object sender, EventArgs e)
        {
            SecureStorage.Default.Remove("ManagerPassword");
        }
    }
}