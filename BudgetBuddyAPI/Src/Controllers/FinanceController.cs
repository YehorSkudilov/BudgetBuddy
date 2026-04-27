using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetBuddyAPI;

[ApiController]
[Route("api/finance")]
public class FinanceController : ControllerBase
{
    private readonly AppDbContext _db;

    public FinanceController(AppDbContext db)
    {
        _db = db;
    }

    private decimal Normalize(decimal amount, BankAccount account)
    {
        var type = account.type?.ToLowerInvariant() ?? "";
        var isLiability = type.Contains("credit") || type.Contains("loan");

        if (!isLiability) return amount;
        return amount > 0 ? -amount : Math.Abs(amount);
    }

    // =========================
    // TOTAL BALANCE
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetTotalBalance()
    {
        var accounts = await _db.BankAccounts.ToListAsync();

        decimal total = accounts.Sum(a => Normalize(a.balances.current, a));

        return Ok(new { total_balance = total });
    }

    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetTotalBalanceByBank(int bankId)
    {
        var accounts = await _db.BankAccounts
            .Where(a => a.bank_connection_id == bankId)
            .Include(a => a.bank_connection)
                .ThenInclude(c => c.institution)
            .ToListAsync();

        if (!accounts.Any())
            return NotFound("No accounts found for this bank.");

        var institution = accounts.First().bank_connection.institution;
        var total = accounts.Sum(a => Normalize(a.balances.current, a));

        return Ok(new
        {
            bank_id = bankId,
            institution_name = institution.name,
            total_balance = total
        });
    }

    // =========================
    // RANGE SUMMARY
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetRangeSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (from == null || to == null)
            return BadRequest(new { message = "Both 'from' and 'to' query parameters are required." });

        if (from >= to)
            return BadRequest(new { message = "'from' must be earlier than 'to'." });

        var transactions = await _db.Transactions
            .Where(t => t.date >= from && t.date < to)
            .ToListAsync();

        decimal income = transactions.Where(t => t.amount < 0).Sum(t => Math.Abs(t.amount));
        decimal expenses = transactions.Where(t => t.amount > 0).Sum(t => t.amount);

        return Ok(new
        {
            income,
            expenses,
            left = income - expenses,
            from,
            to
        });
    }

    // =========================
    // BANKS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllBanks()
    {
        var banks = await _db.BankConnections
            .Include(b => b.institution)
            .Include(b => b.accounts)
            .Select(b => new
            {
                b.id,
                institution_name = b.institution.name,
                b.created_at,
                accounts = b.accounts.Select(a => new
                {
                    a.id,
                    a.account_id,
                    a.name,
                    a.type,
                    a.subtype,
                    a.balances.current,
                    a.balances.available
                })
            })
            .ToListAsync();

        return Ok(banks);
    }

    // =========================
    // ACCOUNTS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await _db.BankAccounts
            .Include(a => a.bank_connection)
                .ThenInclude(c => c.institution)
            .Select(a => new
            {
                a.id,
                a.account_id,
                a.name,
                a.official_name,
                a.type,
                a.subtype,
                a.mask,
                balances = new
                {
                    a.balances.current,
                    a.balances.available,
                    a.balances.iso_currency_code
                },
                bank = new
                {
                    a.bank_connection_id,
                    institution_name = a.bank_connection.institution.name
                }
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetAccountsForBank(int bankId)
    {
        var accounts = await _db.BankAccounts
            .Where(a => a.bank_connection_id == bankId)
            .Select(a => new
            {
                a.id,
                a.account_id,
                a.name,
                a.official_name,
                a.type,
                a.subtype,
                a.mask,
                balances = new
                {
                    a.balances.current,
                    a.balances.available,
                    a.balances.iso_currency_code
                }
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // =========================
    // TRANSACTIONS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var transactions = await _db.Transactions
            .OrderByDescending(t => t.date)
            .Select(t => new
            {
                t.id,
                t.transaction_id,
                t.account_id,

                t.name,
                t.merchant_name,
                t.merchant_entity_id,

                t.amount,
                t.iso_currency_code,
                t.unofficial_currency_code,

                t.date,
                t.authorized_date,
                t.datetime,
                t.authorized_datetime,

                t.pending,
                t.pending_transaction_id,

                t.payment_channel,
                t.transaction_type,
                t.transaction_code,

                t.logo_url,
                t.website,
                t.personal_finance_category_icon_url,

                t.account_owner,
                t.check_number,

                // ---------------- CATEGORY ----------------
                category = t.personal_finance_category == null ? null : new
                {
                    t.personal_finance_category.primary,
                    t.personal_finance_category.detailed,
                    t.personal_finance_category.confidence_level
                },

                // ---------------- COUNTERPARTIES ----------------
                counterparties = t.counterparties.Select(c => new
                {
                    c.Id,
                    c.name,
                    c.entity_id,
                    c.logo_url,
                    c.website,
                    c.type,
                    c.confidence_level,
                    c.phone_number
                }).ToList(),

                // ---------------- LOCATION ----------------
                location = t.location == null ? null : new
                {
                    t.location.address,
                    t.location.city,
                    t.location.region,
                    t.location.country,
                    t.location.postal_code,
                    t.location.lat,
                    t.location.lon,
                    t.location.store_number
                },

                // ---------------- PAYMENT META ----------------
                payment_meta = t.payment_meta == null ? null : new
                {
                    t.payment_meta.by_order_of,
                    t.payment_meta.payee,
                    t.payment_meta.payer,
                    t.payment_meta.payment_method,
                    t.payment_meta.payment_processor,
                    t.payment_meta.ppd_id,
                    t.payment_meta.reason,
                    t.payment_meta.reference_number
                },

                // ---------------- ACCOUNT ----------------
                account = new
                {
                    t.bank_account_id,
                    t.bank_account.name,
                    t.bank_account.bank_connection_id
                },

                // ---------------- BANK ----------------
                bank = new
                {
                    t.bank_account.bank_connection_id,
                    institution_name = t.bank_account.bank_connection.institution.name
                }
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetTransactionsForBank(int bankId)
    {
        var transactions = await _db.Transactions
            .Where(t => t.bank_account.bank_connection_id == bankId)
            .OrderByDescending(t => t.date)
            .Select(t => new
            {
                t.id,
                t.transaction_id,
                t.name,
                t.merchant_name,
                t.amount,
                t.date,
                t.pending,
                t.payment_channel,
                t.logo_url,
                category = t.personal_finance_category == null ? null : new
                {
                    t.personal_finance_category.primary,
                    t.personal_finance_category.detailed
                },
                account = new
                {
                    t.bank_account_id,
                    t.bank_account.name
                }
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("[action]/{accountId}")]
    public async Task<IActionResult> GetTransactionsForAccount(int accountId)
    {
        var transactions = await _db.Transactions
            .Where(t => t.bank_account_id == accountId)
            .OrderByDescending(t => t.date)
            .Select(t => new
            {
                t.id,
                t.transaction_id,
                t.name,
                t.merchant_name,
                t.amount,
                t.date,
                t.pending,
                t.payment_channel,
                t.logo_url,
                category = t.personal_finance_category == null ? null : new
                {
                    t.personal_finance_category.primary,
                    t.personal_finance_category.detailed
                }
            })
            .ToListAsync();

        return Ok(transactions);
    }
}