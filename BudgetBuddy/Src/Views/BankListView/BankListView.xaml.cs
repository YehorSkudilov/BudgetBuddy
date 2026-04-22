using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class BankListView : ContentView
{
    public ObservableCollection<Bank> BankItems { get; set; }
    public BankListView()
    {
        InitializeComponent();

        BankItems = new ObservableCollection<Bank>
{
    new Bank
    {
        BankName = "RBC",
        TotalBalance = 3250.75,
        IsExpanded = false,
        Accounts = new ObservableCollection<Account>
        {
            new Account { Name = "Everyday Chequing", Balance = 1200.50, Type = "Debit" },
            new Account { Name = "Visa Platinum", Balance = -450.25, Type = "Credit Card" },
            new Account { Name = "Savings", Balance = 2500.50, Type = "Savings" }
        }
    },
    new Bank
    {
        BankName = "TD",
        TotalBalance = 1890.00,
        IsExpanded = false,
        Accounts = new ObservableCollection<Account>
        {
            new Account { Name = "Chequing", Balance = 900.00, Type = "Debit" },
            new Account { Name = "TD Rewards Visa", Balance = -210.00, Type = "Credit Card" },
            new Account { Name = "Savings", Balance = 1200.00, Type = "Savings" }
        }
    },
    new Bank
    {
        BankName = "Scotiabank",
        TotalBalance = 5400.25,
        IsExpanded = false,
        Accounts = new ObservableCollection<Account>
        {
            new Account { Name = "Ultimate Package", Balance = 3200.00, Type = "Debit" },
            new Account { Name = "AMEX Gold", Balance = -800.75, Type = "Credit Card" },
            new Account { Name = "Investment", Balance = 3000.00, Type = "Investment" }
        }
    }
};

        MainGrid.BindingContext = this;
    }

    private void OnBankTapped(object sender, EventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Bank bank)
        {
            bank.IsExpanded = !bank.IsExpanded;
        }
    }
}