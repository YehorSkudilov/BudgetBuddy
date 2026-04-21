
using AppSkeleton;
using System.Collections.ObjectModel;
namespace BudgetBuddy;

public partial class CAppShell : ContentPage
{
    public ObservableCollection<CNavItem> CNavIconItems { get; set; }

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


        NavBar.SelectItem(CNavIconItems[2]);

        MainGrid.BindingContext = this;
    }

    void SetContent(ContentView content, string name)
    {
        View.SetContent(content);
        PageName = name;
    }

}

