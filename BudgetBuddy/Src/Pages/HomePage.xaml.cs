using AppSkeleton;
using System.Collections.ObjectModel;
using System.Linq;

namespace BudgetBuddy;

public partial class HomePage : ContentView
{
    public ObservableCollection<TransactionGroup> RecentTransactions { get; set; }

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

    public static readonly BindableProperty MoneyValuesProperty =
        BindableProperty.Create(
            nameof(MoneyValues),
            typeof(ObservableCollection<CChartEntry>),
            typeof(HomePage),
            new ObservableCollection<CChartEntry>());

    public ObservableCollection<CChartEntry> MoneyValues
    {
        get => (ObservableCollection<CChartEntry>)GetValue(MoneyValuesProperty);
        set => SetValue(MoneyValuesProperty, value);
    }

    public static readonly BindableProperty StatsProperty =
        BindableProperty.Create(
            nameof(Stats),
            typeof(ObservableCollection<StatItem>),
            typeof(HomePage),
            new ObservableCollection<StatItem>());

    public ObservableCollection<StatItem> Stats
    {
        get => (ObservableCollection<StatItem>)GetValue(StatsProperty);
        set => SetValue(StatsProperty, value);
    }

    public HomePage()
    {
        InitializeComponent();

        //var sampleTransactions = new List<Transaction>
        //{
        //    new Transaction
        //    {
        //        Name = "Netflix",
        //        Category = "Subscription",
        //        Amount = -16.99,
        //        Date = DateTime.Now
        //    },
        //    new Transaction
        //    {
        //        Name = "Side Job",
        //        Category = "Income",
        //        Amount = 150.00,
        //        Date = DateTime.Now
        //    },
        //    new Transaction
        //    {
        //        Name = "Starbucks",
        //        Category = "Coffee",
        //        Amount = -5.75,
        //        Date = DateTime.Now.AddDays(-1)
        //    },
        //    new Transaction
        //    {
        //        Name = "Starbucks",
        //        Category = "Coffee",
        //        Amount = -5.75,
        //        Date = DateTime.Now.AddDays(-1)
        //    },
        //    new Transaction
        //    {
        //        Name = "Starbucks",
        //        Category = "Coffee",
        //        Amount = -5.75,
        //        Date = DateTime.Now.AddDays(-1)
        //    }
        //};

        //RecentTransactions = new ObservableCollection<TransactionGroup>(
        //    sampleTransactions
        //        .OrderByDescending(t => t.Date)
        //        .GroupBy(t => t.Date.Date)
        //        .Select(g => new TransactionGroup(g.Key, g))
        //);

        Stats = new ObservableCollection<StatItem>
        {
            new StatItem { Title = "Balance", Value = "$4,250", ValueColor = Colors.White },
            new StatItem { Title = "Monthly", Value = "-$1,120", ValueColor = Color.FromArgb("#FF6B6B") },
            new StatItem { Title = "Savings", Value = "28%", ValueColor = Color.FromArgb("#4CD964") }
        };

        Values = new ObservableCollection<CChartEntry>
        {
            new CChartEntry { Label = "2024", Value = 500, Color = Colors.Red },
            new CChartEntry { Label = "2025", Value = 1200, Color = Colors.Green },
            new CChartEntry { Label = "2026", Value = 700, Color = Colors.DodgerBlue }
        };

        MoneyValues = new ObservableCollection<CChartEntry>
        {
            new CChartEntry { Label = "Money", Value = 500, Color = Colors.Red },
            new CChartEntry { Label = "Debt", Value = 1200, Color = Colors.Green },
            new CChartEntry { Label = "Net", Value = 700, Color = Colors.DodgerBlue }
        };

        MainGrid.BindingContext = this;
    }
}