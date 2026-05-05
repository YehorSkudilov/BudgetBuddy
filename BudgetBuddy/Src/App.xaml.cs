using Microsoft.Extensions.DependencyInjection;

namespace BudgetBuddy
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new LoadingPage()); // temporary page

            InitializeAsync(window);

            return window;
        }

        private async void InitializeAsync(Window window)
        {
            // 🔥 Load token BEFORE real UI
            await AuthStore.InitializeAsync();

            // ✅ Now safe to load your app
            MainThread.BeginInvokeOnMainThread(() =>
            {
                window.Page = new CAppShell();
            });
        }
    }
}