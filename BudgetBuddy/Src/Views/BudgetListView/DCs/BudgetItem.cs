using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace BudgetBuddy;
public class BudgetItem : INotifyPropertyChanged
{
    public string Icon { get; set; }
    public string Name { get; set; }

    private double _currentAmount;
    public double CurrentAmount
    {
        get => _currentAmount;
        set
        {
            _currentAmount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Remaining));
        }
    }

    private double _goalAmount;
    public double GoalAmount
    {
        get => _goalAmount;
        set
        {
            _goalAmount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Remaining));
        }
    }

    // 🔥 computed properties (auto update)
    public double Progress => GoalAmount == 0 ? 0 : CurrentAmount / GoalAmount;
    public double Remaining => GoalAmount - CurrentAmount;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}