using System.Collections.ObjectModel;

namespace BudgetBuddy;

public class TransactionGroup : ObservableCollection<Transaction>
{
    public DateTime DateTime { get; set; }

    public TransactionGroup(DateTime dateTime, IEnumerable<Transaction> items)
        : base(items)
    {
        DateTime = dateTime;
    }
}