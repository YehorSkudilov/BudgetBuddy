using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class GoalListView : ContentView
{
    public ObservableCollection<GoalItem> Goals { get; set; }

    public GoalListView()
    {
        InitializeComponent();

        Goals = new ObservableCollection<GoalItem>
        {
            new GoalItem { Icon = "🚗", Name = "Used Car", SavedAmount = 1850, TargetAmount = 6000 },
            new GoalItem { Icon = "✈️", Name = "Mexico Trip", SavedAmount = 920, TargetAmount = 1800 },
            new GoalItem { Icon = "💻", Name = "MacBook", SavedAmount = 400, TargetAmount = 2200 },
            new GoalItem { Icon = "🎮", Name = "Gaming PC", SavedAmount = 1350, TargetAmount = 2500 },
            new GoalItem { Icon = "🏠", Name = "Emergency Fund", SavedAmount = 3200, TargetAmount = 10000 },
            new GoalItem { Icon = "📱", Name = "New Phone", SavedAmount = 780, TargetAmount = 1200 },
            new GoalItem { Icon = "🎓", Name = "Tuition Savings", SavedAmount = 2100, TargetAmount = 8000 },
            new GoalItem { Icon = "🛋️", Name = "Furniture", SavedAmount = 260, TargetAmount = 1500 },
            new GoalItem { Icon = "🚴", Name = "Bike", SavedAmount = 540, TargetAmount = 900 },
            new GoalItem { Icon = "🎁", Name = "Holiday Gifts", SavedAmount = 120, TargetAmount = 600 }
        };

        MainGrid.BindingContext = this;
    }

    private void OnAddToGoalClicked(object sender, EventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is GoalItem goal)
        {
            // Simple example: add fixed amount
            goal.SavedAmount += 50;

            // Notify UI update
            goal.OnPropertyChanged(nameof(goal.SavedAmount));
            goal.OnPropertyChanged(nameof(goal.ProgressValue));
        }
    }
}