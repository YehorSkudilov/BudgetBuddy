using BudgetBuddyAPI;
using Microsoft.EntityFrameworkCore;

[Index(nameof(UserId), nameof(InstitutionId), IsUnique = true)]
public class BankConnection
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string AccessToken { get; set; } = "";
    public string ItemId { get; set; } = "";

    public string InstitutionId { get; set; } = "";
    public string InstitutionName { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public string? TransactionsCursor { get; set; }

    public List<BankAccount> Accounts { get; set; } = new();
}