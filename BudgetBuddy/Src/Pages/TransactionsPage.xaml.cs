using AppSkeleton;
using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class TransactionsPage : ContentView
{
    public ObservableCollection<TransactionGroup> TransactionsGroups { get; set; }

    public static readonly BindableProperty ValuesProperty =
    BindableProperty.Create(
        nameof(Values),
        typeof(ObservableCollection<CChartEntry>),
        typeof(HomePage),
        new ObservableCollection<CChartEntry>());

    public ObservableCollection<CChartEntry> Values
    {
        get => (ObservableCollection<CChartEntry>)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }
    public TransactionsPage()
    {
        InitializeComponent();


        Values = new ObservableCollection<CChartEntry>
        {
            new CChartEntry { Label = "Income", Value = 1200, Color = Colors.Green },
            new CChartEntry { Label = "Spending", Value = 500, Color = Colors.Red },
            new CChartEntry { Label = "Left", Value = 700, Color = Colors.DodgerBlue }
        };

        LoadTransactions();

        MainGrid.BindingContext = this;
    }

    async void LoadTransactions()
    {
        var transactions = await ApiCommunicators.Finance.GetAllTransactionsAsync();

        TransactionsGroups = new ObservableCollection<TransactionGroup>(
            transactions
                .GroupBy(t => t.Date.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TransactionGroup(
                    g.Key,
                    g.OrderByDescending(x => x.Date)
                ))
        );
    }
}