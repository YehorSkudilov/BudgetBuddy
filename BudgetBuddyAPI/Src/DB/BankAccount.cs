using Microsoft.EntityFrameworkCore;

namespace BudgetBuddyAPI;

[Index(nameof(bank_connection_id), nameof(account_id), IsUnique = true)]
public class BankAccount
{
    public int id { get; set; }

    public string account_id { get; set; } = "";
    public string? mask { get; set; }
    public string name { get; set; } = "";
    public string? official_name { get; set; }
    public string type { get; set; } = "";
    public string subtype { get; set; } = "";
    public string? holder_category { get; set; }

    public AccountBalances balances { get; set; } = new();

    public int bank_connection_id { get; set; }
    public BankConnection bank_connection { get; set; } = null!;

    public List<Transaction> transactions { get; set; } = new();
}