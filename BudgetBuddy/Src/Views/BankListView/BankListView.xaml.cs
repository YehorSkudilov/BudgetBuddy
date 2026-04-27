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
}