namespace BudgetBuddyAPI;

public class PlaidAccount
{
    public string account_id { get; set; } = "";
    public string name { get; set; } = "";
    public string type { get; set; } = "";
    public string subtype { get; set; } = "";
    public PlaidBalances balances { get; set; } = new();
}