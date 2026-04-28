
using AppSkeleton;
using System.Collections.ObjectModel;
namespace BudgetBuddy;

public partial class CAppShell : ContentPage
{
    public ObservableCollection<CNavItem> CNavIconItems { get; set; }

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

        CNavIconItems = new ObservableCollection<CNavItem>
        {
            new CNavItem { Glyph = "savings",           Color = Colors.White, Action = () => SetContent(_goalsPage,        "Goals") },
            new CNavItem { Glyph = "receipt",           Color = Colors.White, Action = () => SetContent(_transactionsPage,  "Transactions") },
            new CNavItem { Glyph = "home",              Color = Colors.White, Action = () => SetContent(_homePage,          "Home") },
            new CNavItem { Glyph = "account_balance",   Color = Colors.White, Action = () => SetContent(_banksPage,         "Banks") },
            new CNavItem { Glyph = "account_balance_wallet", Color = Colors.White, Action = () => SetContent(_budgetPage,   "Budget") },
        };

        SettingsAction = () => { SetContent(new SettingsPage(), "Settings"); NavBar.SelectItem(null); };


        MainGrid.BindingContext = this;
    }

    void SetContent(CContentView content, string name)
    {
        TopBar.Deselect();
        content.OnAppearing();
        View.SetContent(content);
        PageName = name;
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        NavBar.SelectItem(CNavIconItems[2]);
    }
}

