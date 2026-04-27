using Microsoft.EntityFrameworkCore;

[Index(nameof(institution_id), IsUnique = true)]
public class Institution
{
    public int id { get; set; }
    public string institution_id { get; set; } = "";
    public string name { get; set; } = "";
    public List<string> country_codes { get; set; } = new();
    public List<string> products { get; set; } = new();
    public DateTime updated_at { get; set; }

    public List<BankConnection> bank_connections { get; set; } = new();
}