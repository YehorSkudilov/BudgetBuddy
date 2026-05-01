namespace BudgetBuddyAPI;

public class User
{
    public Guid id { get; set; }
    public string email { get; set; } = null!;
    public string username { get; set; } = null!;
    public string password { get; set; } = null!;
    public DateTime created_at { get; set; }

    public ICollection<BankConnection> bank_connections { get; set; } = new List<BankConnection>();
}