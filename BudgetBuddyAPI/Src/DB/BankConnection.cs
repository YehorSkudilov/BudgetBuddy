using Microsoft.EntityFrameworkCore;
using BudgetBuddyAPI;

[Index(nameof(user_id), nameof(item_id), IsUnique = true)]
public class BankConnection
{
    public int id { get; set; }

    public string user_id { get; set; } = "";
    public string item_id { get; set; } = "";
    public string access_token { get; set; } = "";

    public int institution_id { get; set; }
    public Institution institution { get; set; } = null!;

    public string? transactions_cursor { get; set; }
    public DateTime created_at { get; set; }

    public List<BankAccount> accounts { get; set; } = new();
}