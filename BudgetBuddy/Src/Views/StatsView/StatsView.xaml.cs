using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class StatsView : ContentView
{
    public static readonly BindableProperty StatsProperty =
        BindableProperty.Create(
            nameof(Stats),
            typeof(ObservableCollection<StatItem>),
            typeof(StatsView),
            new ObservableCollection<StatItem>());

    public ObservableCollection<StatItem> Stats
    {
        get => (ObservableCollection<StatItem>)GetValue(StatsProperty);
        set => SetValue(StatsProperty, value);
    }

    public StatsView()
    {
        InitializeComponent();
        MainGrid.BindingContext = this;
    }
}