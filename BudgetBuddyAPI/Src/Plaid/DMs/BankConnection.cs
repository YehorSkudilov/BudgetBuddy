namespace BudgetBuddyAPI;

public class BankConnection
{
    public int Id { get; set; }
    public string AccessToken { get; set; } = "";
    public string ItemId { get; set; } = "";
    public string InstitutionName { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public string? TransactionsCursor { get; set; }

}