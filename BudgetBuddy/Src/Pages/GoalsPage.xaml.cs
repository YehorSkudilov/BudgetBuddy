using AppSkeleton;
using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class GoalsPage : ContentView
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
    public GoalsPage()
	{
		InitializeComponent();

        Values = new ObservableCollection<CChartEntry>
        {
            new CChartEntry { Label = "Saved", Value = 500, Color = Colors.Red },
            new CChartEntry { Label = "Left", Value = 1200, Color = Colors.Green },
        };

        MainGrid.BindingContext = this;
    }
}