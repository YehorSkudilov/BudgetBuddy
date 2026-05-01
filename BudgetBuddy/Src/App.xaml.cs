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

        protected override async void OnStart()
        {
            base.OnStart();

            await AuthStore.InitializeAsync();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new CAppShell());
        }
    }
}