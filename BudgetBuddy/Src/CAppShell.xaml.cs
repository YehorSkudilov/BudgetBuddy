
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

    public CAppShell()
	{
        InitializeComponent();


        CNavIconItems = new ObservableCollection<CNavItem>
        {
            new CNavItem { Glyph = "savings", Color = Colors.White, Action = () =>{SetContent(new GoalsPage(), "Goals"); } },
            new CNavItem { Glyph = "receipt", Color = Colors.White, Action = () =>{SetContent(new TransactionsPage(), "Transactions"); } },
            new CNavItem { Glyph = "home", Color = Colors.White, Action = () =>{SetContent(new HomePage(), "Home"); } },
            new CNavItem { Glyph = "account_balance", Color = Colors.White, Action = () =>{SetContent(new BanksPage(), "Banks"); } },
            new CNavItem { Glyph = "account_balance_wallet", Color = Colors.White, Action = () =>{SetContent(new BudgetPage(), "Budget"); } }
        };

        SettingsAction = () => { SetContent(new Test(), "Settings"); NavBar.SelectItem(null); };


        MainGrid.BindingContext = this;
    }

    void SetContent(ContentView content, string name)
    {
        TopBar.Deselect();
        View.SetContent(content);
        PageName = name;
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        NavBar.SelectItem(CNavIconItems[2]);
    }
}

