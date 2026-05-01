using AppSkeleton;
using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class CAppShell : ContentPage
{
    public ObservableCollection<CNavItem> CNavIconItems { get; set; }

    private AuthView? _authView;

    public Action NotificationsAction { get; set; }
    public Action SettingsAction { get; set; }

    private string pageName;

    public string PageName
    {
        get => pageName;
        set
        {
            if (pageName == value) return;
            pageName = value;
            OnPropertyChanged();
        }
    }

    private readonly GoalsPage _goalsPage = new();
    private readonly TransactionsPage _transactionsPage = new();
    private readonly HomePage _homePage = new();
    private readonly BanksPage _banksPage = new();
    private readonly BudgetPage _budgetPage = new();

    public CAppShell()
    {
        InitializeComponent();

        // NAV ITEMS (main app pages stay reused — fine)
        CNavIconItems = new ObservableCollection<CNavItem>
        {
            new CNavItem { Glyph = "savings", Action = () => SetContent(new GoalsPage(), "Goals") },
            new CNavItem { Glyph = "receipt", Action = () => SetContent(new TransactionsPage(), "Transactions") },
            new CNavItem { Glyph = "home", Action = () => SetContent(new HomePage(), "Home") },
            new CNavItem { Glyph = "account_balance", Action = () => SetContent(new BanksPage(), "Banks") },
            new CNavItem { Glyph = "account_balance_wallet", Action = () => SetContent(new BudgetPage(), "Budget") },
        };

        SettingsAction = () =>
        {
            SetContent(new SettingsPage(), "Settings");
            NavBar.SelectItem(null);
        };

        MainGrid.BindingContext = this;

        // 🔐 auth state listener
        AuthStore.AuthChanged += OnLoggedOut;
    }

    // ---------------- AUTH STATE CHANGE ----------------
    private void OnLoggedOut()
    {
        MainThread.BeginInvokeOnMainThread(ShowAuth);
    }

    // ---------------- PAGE LOAD ----------------
    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await CheckAuth();

        if (!AuthContainer.IsVisible)
        {
            NavBar.SelectItem(CNavIconItems[2]);
        }
    }

    // ---------------- AUTH CHECK ----------------
    private async Task CheckAuth()
    {
        if (string.IsNullOrEmpty(AuthStore.Token))
        {
            ShowAuth();
            return;
        }

        try
        {
            await ApiCommunicators.User.GetUserAsync();
        }
        catch
        {
            AuthStore.Logout();
        }
    }

    // ---------------- SHOW LOGIN ----------------
    private void ShowAuth()
    {
        AuthContainer.IsVisible = true;
        MainGrid.IsEnabled = false;

        AuthContainer.Children.Clear();

        _authView = new AuthView();
        _authView.Authenticated += OnAuthenticated;

        AuthContainer.Children.Add(_authView);
    }

    // ---------------- LOGIN SUCCESS ----------------
    private void OnAuthenticated()
    {
        AuthContainer.IsVisible = false;
        MainGrid.IsEnabled = true;

        AuthContainer.Children.Clear();

        NavBar.SelectItem(CNavIconItems[2]);
    }

    // ---------------- CONTENT SWITCH ----------------
    void SetContent(CContentView content, string name)
    {
        TopBar.Deselect();

        // ❌ DO NOT call OnAppearing manually (MAUI handles this)
        View.SetContent(content);

        PageName = name;
    }
}