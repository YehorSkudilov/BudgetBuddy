using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class BudgetListView : ContentView
{
    public ObservableCollection<BudgetItem> BudgetItems { get; set; }

    public BudgetListView()
    {
        InitializeComponent();

        BudgetItems = new ObservableCollection<BudgetItem>
        {
            new BudgetItem { Icon = "🚗", Name = "Traveling", CurrentAmount = 90, GoalAmount = 150 },
            new BudgetItem { Icon = "🍇", Name = "Groceries", CurrentAmount = 45, GoalAmount = 300 },
            new BudgetItem { Icon = "🍔", Name = "Food & Dining", CurrentAmount = 120.5, GoalAmount = 250 },
            new BudgetItem { Icon = "🏠", Name = "Rent", CurrentAmount = 1200, GoalAmount = 1200 },
            new BudgetItem { Icon = "🎮", Name = "Entertainment", CurrentAmount = 35, GoalAmount = 100 },
            new BudgetItem { Icon = "💡", Name = "Utilities", CurrentAmount = 80, GoalAmount = 200 },
            new BudgetItem { Icon = "🛍️", Name = "Shopping", CurrentAmount = 210, GoalAmount = 300 },
            new BudgetItem { Icon = "📱", Name = "Subscriptions", CurrentAmount = 25, GoalAmount = 60 },
            new BudgetItem { Icon = "💊", Name = "Health", CurrentAmount = 40, GoalAmount = 150 },
            new BudgetItem { Icon = "🎓", Name = "Education", CurrentAmount = 75, GoalAmount = 500 },
            new BudgetItem { Icon = "🚕", Name = "Transport", CurrentAmount = 60, GoalAmount = 120 },
            new BudgetItem { Icon = "🎁", Name = "Gifts", CurrentAmount = 20, GoalAmount = 100 }
        };

        MainGrid.BindingContext = this;
    }
}