using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class BudgetListView : ContentView
{
    public ObservableCollection<object> BudgetItems { get; set; }

    public BudgetListView()
    {
        InitializeComponent();

        BudgetItems = new ObservableCollection<object>
        {
            new {
                Icon = "🚗",
                Name = "Traveling",
                CurrentAmount = 90.00,
                GoalAmount = 150.00,
                Progress = 0.60,
                Remaining = 60.00
            },
            new {
                Icon = "🍇",
                Name = "Groceries",
                CurrentAmount = 45.00,
                GoalAmount = 300.00,
                Progress = 0.15,
                Remaining = 255.00
            },
            new {
                Icon = "🍔",
                Name = "Food & Dining",
                CurrentAmount = 120.50,
                GoalAmount = 250.00,
                Progress = 0.48,
                Remaining = 129.50
            },
            new {
                Icon = "🏠",
                Name = "Rent",
                CurrentAmount = 1200.00,
                GoalAmount = 1200.00,
                Progress = 1.00,
                Remaining = 0.00
            },
            new {
                Icon = "🎮",
                Name = "Entertainment",
                CurrentAmount = 35.00,
                GoalAmount = 100.00,
                Progress = 0.35,
                Remaining = 65.00
            },
            new {
                Icon = "💡",
                Name = "Utilities",
                CurrentAmount = 80.00,
                GoalAmount = 200.00,
                Progress = 0.40,
                Remaining = 120.00
            },
            new {
                Icon = "🛍️",
                Name = "Shopping",
                CurrentAmount = 210.00,
                GoalAmount = 300.00,
                Progress = 0.70,
                Remaining = 90.00
            },
            new {
                Icon = "📱",
                Name = "Subscriptions",
                CurrentAmount = 25.00,
                GoalAmount = 60.00,
                Progress = 0.42,
                Remaining = 35.00
            },
            new {
                Icon = "💊",
                Name = "Health",
                CurrentAmount = 40.00,
                GoalAmount = 150.00,
                Progress = 0.27,
                Remaining = 110.00
            },
            new {
                Icon = "🎓",
                Name = "Education",
                CurrentAmount = 75.00,
                GoalAmount = 500.00,
                Progress = 0.15,
                Remaining = 425.00
            },
            new {
                Icon = "🚕",
                Name = "Transport",
                CurrentAmount = 60.00,
                GoalAmount = 120.00,
                Progress = 0.50,
                Remaining = 60.00
            },
            new {
                Icon = "🎁",
                Name = "Gifts",
                CurrentAmount = 20.00,
                GoalAmount = 100.00,
                Progress = 0.20,
                Remaining = 80.00
            }
        };

        MainGrid.BindingContext = this;
    }
}