using AppSkeleton;
using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class HomePage : ContentView
{
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

    public HomePage()
    {
        InitializeComponent();

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