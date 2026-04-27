using BudgetBuddyAPI;
using BudgetBuddyAPI.Src.DB;
using Microsoft.EntityFrameworkCore;

[Index(nameof(transaction_id), IsUnique = true)]
public class Transaction
{
    public Guid id { get; set; }

    public string transaction_id { get; set; } = "";
    public string account_id { get; set; } = "";
    public decimal amount { get; set; }
    public string name { get; set; } = "";
    public string? merchant_name { get; set; }
    public string? merchant_entity_id { get; set; }
    public string? iso_currency_code { get; set; }
    public string? unofficial_currency_code { get; set; }
    public DateTime date { get; set; }
    public DateTime? authorized_date { get; set; }
    public DateTime? datetime { get; set; }
    public DateTime? authorized_datetime { get; set; }
    public bool pending { get; set; }
    public string? pending_transaction_id { get; set; }
    public string? payment_channel { get; set; }
    public string? transaction_type { get; set; }
    public string? transaction_code { get; set; }
    public string? logo_url { get; set; }
    public string? website { get; set; }
    public string? personal_finance_category_icon_url { get; set; }
    public string? account_owner { get; set; }
    public string? check_number { get; set; }

    // Navigation: personal_finance_category object
    public PersonalFinanceCategory? personal_finance_category { get; set; }

    // Navigation: counterparties array
    public List<TransactionCounterparty> counterparties { get; set; } = new();

    // Navigation: location object
    public TransactionLocation? location { get; set; }

    // Navigation: payment_meta object
    public TransactionPaymentMeta? payment_meta { get; set; }

    public int bank_account_id { get; set; }
    public BankAccount bank_account { get; set; } = null!;
}