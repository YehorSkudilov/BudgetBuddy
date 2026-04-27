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
            typeof(ObservableCollection<Bank>),
            typeof(BankListView),
            new ObservableCollection<Bank>());

    public ObservableCollection<Bank> BankItems
    {
        get => (ObservableCollection<Bank>)GetValue(BankItemsProperty);
        set => SetValue(BankItemsProperty, value);
    }

    private void OnBankTapped(object sender, EventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Bank bank)
        {
            bank.IsExpanded = !bank.IsExpanded;
        }
    }
}