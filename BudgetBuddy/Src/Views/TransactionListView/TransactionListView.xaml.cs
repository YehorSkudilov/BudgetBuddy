using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class TransactionListView : ContentView
{
    public static readonly BindableProperty ItemsProperty =
        BindableProperty.Create(
            nameof(Items),
            typeof(ObservableCollection<TransactionGroup>),
            typeof(TransactionListView),
            new ObservableCollection<TransactionGroup>()
        );

    public ObservableCollection<TransactionGroup> Items
    {
        get => (ObservableCollection<TransactionGroup>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public TransactionListView()
    {
        InitializeComponent();
        MainGrid.BindingContext = this;
    }
}