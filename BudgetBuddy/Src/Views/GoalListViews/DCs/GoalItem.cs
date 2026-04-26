using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BudgetBuddy;

public class GoalItem : INotifyPropertyChanged
{
    private double savedAmount;

    public string Icon { get; set; }
    public string Name { get; set; }

    public double TargetAmount { get; set; }

    public double SavedAmount
    {
        get => savedAmount;
        set
        {
            if (savedAmount != value)
            {
                savedAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressValue));
            }
        }
    }

    public double ProgressValue =>
        TargetAmount == 0 ? 0 : SavedAmount / TargetAmount;

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}