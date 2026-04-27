using Microsoft.EntityFrameworkCore;

[Owned]
public class TransactionCounterparty
{
    public string? name { get; set; }
    public string? entity_id { get; set; }
    public string? logo_url { get; set; }
    public string? website { get; set; }
    public string? type { get; set; }
    public string? confidence_level { get; set; }
    public string? phone_number { get; set; }
}