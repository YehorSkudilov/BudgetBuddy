using AppSkeleton;
using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class BudgetPage : ContentView
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


    public BudgetPage()
    {
        InitializeComponent();

        Values = new ObservableCollection<CChartEntry>
        {
            new CChartEntry { Label = "Food", Value = 500, Color = Colors.Red },
            new CChartEntry { Label = "Rent", Value = 1200, Color = Colors.Green },
            new CChartEntry { Label = "Save", Value = 700, Color = Colors.DodgerBlue }
        };


        MainGrid.BindingContext = this;
    }
}