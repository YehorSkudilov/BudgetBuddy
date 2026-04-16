namespace BudgetBuddyAPI;

public class PlaidSyncResponse
{
    public List<TransactionResponse> added { get; set; }
    public List<TransactionResponse> modified { get; set; }
    public List<TransactionResponse> removed { get; set; }

    public string next_cursor { get; set; }
    public bool has_more { get; set; }
}