using System;

namespace BudgetBuddy;

public class Transaction
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = "";

    public string Name { get; set; } = "";
    public double Amount { get; set; }

    public DateTime Date { get; set; }
    public DateTime? DateTime { get; set; }


    public bool Pending { get; set; }

    public string? PaymentChannel { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }

    // FIX: category is an OBJECT, not string
    public TransactionCategory? Category { get; set; }

    public Account? Account { get; set; }
    public BankConnection? Bank { get; set; }
}