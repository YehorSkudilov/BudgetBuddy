using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBuddy;


public class Transaction
{
    public int Id { get; set; }
    public string PlaidTransactionId { get; set; }
    public string Name { get; set; }
    public double Amount { get; set; }
    public string? Category { get; set; }
    public DateTime Date { get; set; }

    public Account Account { get; set; }
    public BankConnection Bank { get; set; }
}