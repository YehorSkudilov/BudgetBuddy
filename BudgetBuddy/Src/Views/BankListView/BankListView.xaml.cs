using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class BankListView : ContentView
{
    public BankListView()
    {
        InitializeComponent();
        MainGrid.BindingContext = this;
    }

    // -----------------------------
    // BINDABLE SOURCE PROPERTY
    // -----------------------------
    public static readonly BindableProperty BankItemsProperty =
        BindableProperty.Create(
            nameof(BankItems),
            typeof(ObservableCollection<BankConnection>),
            typeof(BankListView),
            new ObservableCollection<BankConnection>());

    public ObservableCollection<BankConnection> BankItems
    {
        get => (ObservableCollection<BankConnection>)GetValue(BankItemsProperty);
        set => SetValue(BankItemsProperty, value);
    }

    private void OnBankTapped(object sender, EventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is BankConnection bank)
        {
            bank.IsExpanded = !bank.IsExpanded;
        }
    }

    private async void CButton_Clicked(object sender, EventArgs e)
    {
        if (sender is AppSkeleton.CButton button &&
            button.BindingContext is BankConnection bank)
        {
            // you now have the bank object for that row
            var bankId = bank.Id; // or whatever your PK is

            var plaid = new Plaid();
            await plaid.RemoveBankAsync(bankId);

            // optional: remove from UI immediately
            BankItems.Remove(bank);
        }
    }
}