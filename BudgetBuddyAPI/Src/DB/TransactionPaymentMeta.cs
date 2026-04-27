namespace BudgetBuddyAPI;
public class TransactionPaymentMeta
{
    public int id { get; set; }

    public string? by_order_of { get; set; }
    public string? payee { get; set; }
    public string? payer { get; set; }
    public string? payment_method { get; set; }
    public string? payment_processor { get; set; }
    public string? ppd_id { get; set; }
    public string? reason { get; set; }
    public string? reference_number { get; set; }

    public Guid transaction_id { get; set; }
    public Transaction transaction { get; set; } = null!;
}