namespace BudgetBuddyAPI;
public class TransactionLocation
{
    public int id { get; set; }

    public string? address { get; set; }
    public string? city { get; set; }
    public string? region { get; set; }
    public string? country { get; set; }
    public string? postal_code { get; set; }
    public double? lat { get; set; }
    public double? lon { get; set; }
    public string? store_number { get; set; }

    public Guid transaction_id { get; set; }
    public Transaction transaction { get; set; } = null!;
}

