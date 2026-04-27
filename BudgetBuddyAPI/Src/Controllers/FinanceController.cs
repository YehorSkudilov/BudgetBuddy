using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

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
        var type = account.Type?.ToLowerInvariant() ?? "";
        var subtype = account.Subtype?.ToLowerInvariant() ?? "";

        var isLiability =
            type.Contains("credit") ||
            type.Contains("loan");


        if (!isLiability)
            return amount;


        return amount > 0 ? -amount : Math.Abs(amount);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetTotalBalance()
    {
        var accounts = await _db.BankAccounts.ToListAsync();

        decimal total = accounts.Sum(a =>
            Normalize(a.Balance ?? 0, a)
        );

        return Ok(new
        {
            totalBalance = total
        });
    }


    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetTotalBalanceByBank(int bankId)
    {
        var accounts = await _db.BankAccounts
            .Where(a => a.BankConnectionId == bankId)
            .Include(a => a.BankConnection)
            .ToListAsync();

        if (!accounts.Any())
        {
            return NotFound("No accounts found for this bank.");
        }

        var bankName = accounts.First().BankConnection.InstitutionName;

        var totalBalance = accounts.Sum(a =>
            Normalize(a.Balance ?? 0, a)
        );

        return Ok(new
        {
            bankId,
            bankName,
            totalBalance
        });
    }

    // =========================
    // GET CURRENT MONTH SUMMARY
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetRangeSummary(
     [FromQuery] DateTime? from,
     [FromQuery] DateTime? to)
    {
        if (from == null || to == null)
        {
            return BadRequest(new
            {
                message = "Both 'from' and 'to' query parameters are required."
            });
        }

        if (from >= to)
        {
            return BadRequest(new
            {
                message = "'from' must be earlier than 'to'."
            });
        }

        var transactions = await _db.Transactions
            .Where(t => t.Date >= from && t.Date < to)
            .ToListAsync();

        decimal income = transactions
            .Where(t => t.Amount < 0)
            .Sum(t => Math.Abs(t.Amount));

        decimal expenses = transactions
            .Where(t => t.Amount > 0)
            .Sum(t => t.Amount);

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
    // GET BANKS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllBanks()
    {
        var banks = await _db.BankConnections
            .Include(b => b.Accounts)
            .Select(b => new
            {
                b.Id,
                b.InstitutionName,
                b.CreatedAt,
                Accounts = b.Accounts.Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Type,
                    a.Subtype,
                    a.Balance
                })
            })
            .ToListAsync();

        return Ok(banks);
    }


    // =========================
    // ACCOUNTS
    // =========================
    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetAccountsForBank(int bankId)
    {
        var accounts = await _db.BankAccounts
            .Where(a => a.BankConnectionId == bankId)
            .Select(a => new
            {
                a.Id,
                a.PlaidAccountId,
                a.Name,
                a.Type,
                a.Subtype,
                a.Balance
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await _db.BankAccounts
            .Include(a => a.BankConnection)
            .Select(a => new
            {
                a.Id,
                a.PlaidAccountId,
                a.Name,
                a.Type,
                a.Subtype,
                a.Balance,
                Bank = new
                {
                    a.BankConnectionId,
                    a.BankConnection.InstitutionName
                }
            })
            .ToListAsync();

        return Ok(accounts);
    }

    // =========================
    // TRANSACTIONS
    // =========================
    [HttpGet("[action]/{accountId}")]
    public async Task<IActionResult> GetTransactionsForAccount(int accountId)
    {
        var transactions = await _db.Transactions
            .Where(t => t.BankAccountId == accountId)
            .OrderByDescending(t => t.Date)
            .Select(t => new
            {
                t.Id,
                t.PlaidTransactionId,
                t.Name,
                t.Amount,
                t.Category,
                t.Date
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetTransactionsForBank(int bankId)
    {
        var transactions = await _db.Transactions
            .Where(t => t.BankAccount.BankConnectionId == bankId)
            .OrderByDescending(t => t.Date)
            .Select(t => new
            {
                t.Id,
                t.PlaidTransactionId,
                t.Name,
                t.Amount,
                t.Category,
                t.Date,
                Account = new
                {
                    t.BankAccountId,
                    t.BankAccount.Name
                }
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var transactions = await _db.Transactions
            .Include(t => t.BankAccount)
            .ThenInclude(a => a.BankConnection)
            .OrderByDescending(t => t.Date)
            .Select(t => new
            {
                t.Id,
                t.PlaidTransactionId,
                t.Name,
                t.Amount,
                t.Category,
                t.Date,
                Account = new
                {
                    t.BankAccountId,
                    t.BankAccount.Name
                },
                Bank = new
                {
                    t.BankAccount.BankConnectionId,
                    t.BankAccount.BankConnection.InstitutionName
                }
            })
            .ToListAsync();

        return Ok(transactions);
    }
}