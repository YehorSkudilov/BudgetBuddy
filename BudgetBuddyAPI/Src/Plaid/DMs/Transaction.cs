namespace BudgetBuddyAPI;

public class Transaction
{
    public int Id { get; set; }

    public string PlaidTransactionId { get; set; } = "";

    public decimal Amount { get; set; }
    public string Name { get; set; } = "";
    public string? Category { get; set; }

    public DateTime Date { get; set; }

    public int BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
}