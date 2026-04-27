using BudgetBuddyAPI;
using BudgetBuddyAPI.Src.DB;
using Microsoft.EntityFrameworkCore;

[Index(nameof(transaction_id), IsUnique = true)]
public class Transaction
{
    public int id { get; set; }

    public string transaction_id { get; set; } = "";
    public string account_id { get; set; } = "";
    public decimal amount { get; set; }
    public string name { get; set; } = "";
    public string? merchant_name { get; set; }
    public string? iso_currency_code { get; set; }
    public DateTime date { get; set; }
    public DateTime? authorized_date { get; set; }
    public bool pending { get; set; }
    public string? payment_channel { get; set; }
    public string? transaction_type { get; set; }
    public string? logo_url { get; set; }
    public string? website { get; set; }

    // Flattened from personal_finance_category
    public PersonalFinanceCategory? personal_finance_category { get; set; }

    // Flattened from counterparties[0]
    public List<TransactionCounterparty> counterparties { get; set; } = new();

    public int bank_account_id { get; set; }
    public BankAccount bank_account { get; set; } = null!;
}