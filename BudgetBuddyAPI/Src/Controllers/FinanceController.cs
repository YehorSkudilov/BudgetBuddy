using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetBuddyAPI;

[Authorize]
[ApiController]
[Route("api/finance")]
public class FinanceController : BaseController
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

        return isLiability
            ? (amount > 0 ? -amount : Math.Abs(amount))
            : amount;
    }

    // =========================
    // TOTAL BALANCE
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetTotalBalance()
    {
        var userId = UserId;

        var accounts = await _db.BankAccounts
            .Where(a => a.bank_connection.user_id == userId)
            .ToListAsync();

        var total = accounts.Sum(a => Normalize(a.balances.current, a));

        return Ok(new { total_balance = total });
    }

    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetTotalBalanceByBank(int bankId)
    {
        var userId = UserId;

        var accounts = await _db.BankAccounts
            .Where(a => a.bank_connection_id == bankId &&
                        a.bank_connection.user_id == userId)
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
    public async Task<IActionResult> GetRangeSummary(DateTime? from, DateTime? to)
    {
        if (from == null || to == null)
            return BadRequest("from/to required");

        var userId = UserId;

        var transactions = await _db.Transactions
            .Where(t => t.date >= from && t.date < to &&
                        t.bank_account.bank_connection.user_id == userId)
            .ToListAsync();

        var income = transactions.Where(t => t.amount < 0).Sum(t => Math.Abs(t.amount));
        var expenses = transactions.Where(t => t.amount > 0).Sum(t => t.amount);

        return Ok(new
        {
            income,
            expenses,
            left = income - expenses
        });
    }

    // =========================
    // BANKS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllBanks()
    {
        var userId = UserId;

        var banks = await _db.BankConnections
            .Where(b => b.user_id == userId)
            .Include(b => b.institution)
            .Include(b => b.accounts)
            .Select(b => new
            {
                b.id,
                institution_name = b.institution.name,
                accounts = b.accounts.Select(a => new
                {
                    a.id,
                    a.name,
                    a.type,
                    a.balances.current
                })
            })
            .ToListAsync();

        return Ok(banks);
    }

    // =========================
    // TRANSACTIONS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var userId = UserId;

        var transactions = await _db.Transactions
            .Where(t => t.bank_account.bank_connection.user_id == userId)
            .OrderByDescending(t => t.date)
            .ToListAsync();

        return Ok(transactions);
    }
}