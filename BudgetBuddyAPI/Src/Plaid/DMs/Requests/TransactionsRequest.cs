namespace BudgetBuddyAPI;

public class TransactionsRequest
{
    public string access_token { get; set; }
    public string start_date { get; set; }
    public string end_date { get; set; }
}
