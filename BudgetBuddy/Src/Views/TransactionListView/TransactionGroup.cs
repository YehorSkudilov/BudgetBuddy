using System.Collections.ObjectModel;

namespace BudgetBuddy;

public class TransactionGroup : ObservableCollection<TransactionItem>
{
    public DateTime DateTime { get; set; }

    public TransactionGroup(DateTime dateTime, IEnumerable<TransactionItem> items)
        : base(items)
    {
        DateTime = dateTime;
    }
}