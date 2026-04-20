using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBuddy;


public class TransactionItem
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public bool IsIncome { get; set; }
}