using Microsoft.EntityFrameworkCore;

namespace BudgetBuddyAPI;

[Index(nameof(BankConnectionId), nameof(PlaidAccountId), IsUnique = true)]
public class BankAccount
{
    public int Id { get; set; }

    public string PlaidAccountId { get; set; } = "";

    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Subtype { get; set; } = "";

    public decimal? Balance { get; set; }

    public int BankConnectionId { get; set; }
    public BankConnection BankConnection { get; set; } = null!;

    public List<Transaction> Transactions { get; set; } = new();
}