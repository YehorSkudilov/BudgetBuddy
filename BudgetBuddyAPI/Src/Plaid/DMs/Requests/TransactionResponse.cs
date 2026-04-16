namespace BudgetBuddyAPI;

public class TransactionResponse
{
    public string transaction_id { get; set; } = "";

    public string account_id { get; set; } = "";

    public decimal amount { get; set; }
    public string name { get; set; } = "";
    public List<string>? category { get; set; }
    public string date { get; set; } = "";
}