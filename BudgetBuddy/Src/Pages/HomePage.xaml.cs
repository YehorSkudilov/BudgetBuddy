using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class HomePage : ContentView
{
    public static readonly BindableProperty ValuesProperty =
        BindableProperty.Create(
            nameof(Values),
            typeof(ObservableCollection<float>),
            typeof(HomePage),
            new ObservableCollection<float>());

    public ObservableCollection<float> Values
    {
        get => (ObservableCollection<float>)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public HomePage()
    {
        InitializeComponent();

        Values = new ObservableCollection<float>
        {
            10, 30, 20, 50, 40, 80, 60
        };

        MainGrid.BindingContext = this;
    }
}