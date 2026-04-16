using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetBuddyAPI;

[ApiController]
[Route("api/plaid")]
public class PlaidController : ControllerBase
{
    private readonly PlaidClient _plaid;
    private readonly AppDbContext _db;

    public PlaidController(PlaidClient plaid, AppDbContext db)
    {
        _plaid = plaid;
        _db = db;
    }

    // =========================
    // CREATE LINK TOKEN
    // =========================
    [HttpPost("[action]")]
    public async Task<IActionResult> CreateLinkToken()
    {
        var res = await _plaid.CreateLinkTokenAsync(new LinkTokenRequest
        {
            user = new { client_user_id = Guid.NewGuid().ToString() },
            client_name = "BudgetBuddy",
            products = new List<string> { "transactions" },
            country_codes = new List<string> { "CA" },
            language = "en"
        });

        return Ok(res);
    }

    // =========================
    // EXCHANGE PUBLIC TOKEN → SAVE BANK
    // =========================
    [HttpPost("[action]")]
    public async Task<IActionResult> Exchange([FromBody] ExchangeTokenRequest req)
    {
        var res = await _plaid.ExchangePublicTokenAsync(req);

        string institutionName = "Unknown";

        try
        {
            var item = await _plaid.GetItemAsync(res.access_token);
            var institution = await _plaid.GetInstitutionAsync(item.item.institution_id);
            institutionName = institution.institution.name;
        }
        catch { }

        var bank = new BankConnection
        {
            AccessToken = res.access_token,
            ItemId = res.item_id,
            InstitutionName = institutionName,
            CreatedAt = DateTime.UtcNow
        };

        _db.BankConnections.Add(bank);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            bank.Id,
            bank.InstitutionName
        });
    }

    // =========================
    // REMOVE BANK
    // =========================
    [HttpDelete("[action]/{bankId}")]
    public async Task<IActionResult> RemoveBank(int bankId)
    {
        var bank = await _db.BankConnections
            .FirstOrDefaultAsync(x => x.Id == bankId);

        if (bank == null)
            return NotFound("Bank not found");

        var transactions = _db.Transactions
            .Where(t => t.BankConnectionId == bankId);

        _db.Transactions.RemoveRange(transactions);
        _db.BankConnections.Remove(bank);

        await _db.SaveChangesAsync();

        return Ok(new { message = "bank + transactions removed" });
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
    // SYNC ENTRYPOINT
    // =========================
    private async Task<object> SyncBankAsync(BankConnection bank)
    {
        var result = await SyncTransactionsAsync(bank);

        bank.TransactionsCursor = result.cursor;
        await _db.SaveChangesAsync();

        return new
        {
            added = result.added,
            modified = result.modified,
            removed = result.removed,
            cursor = result.cursor
        };
    }


    // =========================
    // TRANSACTIONS SYNC (SECOND)
    // =========================
    private async Task<(int added, int modified, int removed, string cursor)>
        SyncTransactionsAsync(BankConnection bank)
    {
        int added = 0, modified = 0, removed = 0;

        var cursor = bank.TransactionsCursor;
        bool hasMore = true;

        var existingSet = await _db.Transactions
            .Where(x => x.BankConnectionId == bank.Id)
            .Select(x => x.PlaidTransactionId)
            .ToHashSetAsync();


        while (hasMore)
        {
            var res = await _plaid.PostSyncAsync(cursor, bank.AccessToken);

            // ADD
            foreach (var t in res.added)
            {
                if (existingSet.Contains(t.transaction_id))
                    continue;


                _db.Transactions.Add(new Transaction
                {
                    PlaidTransactionId = t.transaction_id,
                    Amount = t.amount,
                    Name = t.name,
                    Category = t.category != null ? string.Join(",", t.category) : null,
                    Date = DateTime.Parse(t.date),

                    BankConnectionId = bank.Id,
                });

                existingSet.Add(t.transaction_id);
                added++;
            }

            // MODIFY
            foreach (var t in res.modified)
            {
                var existing = await _db.Transactions
                    .FirstOrDefaultAsync(x => x.PlaidTransactionId == t.transaction_id);

                if (existing != null)
                {
                    existing.Amount = t.amount;
                    existing.Name = t.name;
                    existing.Category = t.category != null ? string.Join(",", t.category) : null;
                    existing.Date = DateTime.Parse(t.date);

                    modified++;
                }
            }

            // REMOVE
            foreach (var t in res.removed)
            {
                var existing = await _db.Transactions
                    .FirstOrDefaultAsync(x => x.PlaidTransactionId == t.transaction_id);

                if (existing != null)
                {
                    _db.Transactions.Remove(existing);
                    removed++;
                }
            }

            cursor = res.next_cursor;
            hasMore = res.has_more;
        }

        return (added, modified, removed, cursor);
    }

    // =========================
    // SYNC ONE BANK
    // =========================
    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> SyncBank(int bankId)
    {
        var bank = await _db.BankConnections
            .FirstOrDefaultAsync(x => x.Id == bankId);

        if (bank == null)
            return NotFound("Bank not found");

        var result = await SyncBankAsync(bank);

        return Ok(new
        {
            message = "synced",
            bankId,
            result
        });
    }

    // =========================
    // SYNC ALL BANKS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> SyncAllBanks()
    {
        var banks = await _db.BankConnections.ToListAsync();
        var results = new List<object>();

        foreach (var bank in banks)
        {
            results.Add(new
            {
                bank.Id,
                bank.InstitutionName,
                result = await SyncBankAsync(bank)
            });
        }

        return Ok(new
        {
            message = "all banks synced",
            count = banks.Count,
            results
        });
    }

    // =========================
    // GET TRANSACTIONS (BANK)
    // =========================
    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> GetTransactionsForBank(int bankId)
    {
        var transactions = await _db.Transactions
            .Where(t => t.BankConnectionId == bankId)
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

    // =========================
    // GET TRANSACTIONS (ACCOUNT)
    // =========================
    [HttpGet("[action]/{accountId}")]
    public async Task<IActionResult> GetTransactionsForAccount(int accountId)
    {
        var transactions = await _db.Transactions
            .Where(t => t.AccountId == accountId)
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

    // =========================
    // GET ALL TRANSACTIONS
    // =========================
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var transactions = await _db.Transactions
            .Include(t => t.BankConnection)
            .OrderByDescending(t => t.Date)
            .Select(t => new
            {
                t.Id,
                t.PlaidTransactionId,
                t.Name,
                t.Amount,
                t.Category,
                t.Date,
                bank = new
                {
                    t.BankConnectionId,
                    t.BankConnection.InstitutionName
                }
            })
            .ToListAsync();

        return Ok(transactions);
    }

}