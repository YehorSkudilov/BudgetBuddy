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

    [HttpGet("total-balance")]
    public async Task<IActionResult> GetTotalBalance()
    {
        var total = await _db.BankAccounts
            .SumAsync(a => (decimal?)a.Balance) ?? 0;

        return Ok(new
        {
            totalBalance = total
        });
    }

    [HttpGet("total-balance-by-bank")]
    public async Task<IActionResult> GetTotalBalanceByBank()
    {
        var result = await _db.BankAccounts
            .Include(a => a.BankConnection)
            .GroupBy(a => new
            {
                a.BankConnectionId,
                a.BankConnection.InstitutionName
            })
            .Select(g => new
            {
                bankId = g.Key.BankConnectionId,
                bankName = g.Key.InstitutionName,
                totalBalance = g.Sum(x => x.Balance)
            })
            .ToListAsync();

        return Ok(result);
    }

    // =========================
    // GET CURRENT MONTH SUMMARY
    // =========================
    [HttpGet("monthly-summary")]
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
    public async Task<IActionResult> GetBanks()
    {
        var banks = await _db.BankConnections
            .OrderByDescending(x => x.CreatedAt)
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