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

    [HttpPost("[action]")]
    public async Task<IActionResult> Exchange([FromBody] ExchangeTokenRequest req)
    {
        var res = await _plaid.ExchangePublicTokenAsync(req);

        string institutionId = "";
        string institutionName = "Unknown";

        try
        {
            var item = await _plaid.GetItemAsync(res.access_token);

            institutionId = item.item.institution_id;

            var institution = await _plaid.GetInstitutionAsync(institutionId);
            institutionName = institution.institution.name;
        }
        catch { }

        // =========================
        // 🔥 PREVENT DUPLICATES HERE
        // =========================
        var existing = await _db.BankConnections
            .FirstOrDefaultAsync(x =>
                x.InstitutionId == institutionId
            );

        if (existing != null)
        {
            // Optional: update tokens if re-linked
            existing.AccessToken = res.access_token;
            existing.ItemId = res.item_id;
            existing.InstitutionName = institutionName;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Bank already exists, updated connection",
                existing.Id,
                existing.InstitutionName
            });
        }

        var bank = new BankConnection
        {
            AccessToken = res.access_token,
            ItemId = res.item_id,
            InstitutionId = institutionId,
            InstitutionName = institutionName,
            CreatedAt = DateTime.UtcNow
        };

        _db.BankConnections.Add(bank);
        await _db.SaveChangesAsync();

        return Ok(new { bank.Id, bank.InstitutionName });
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

        var accounts = _db.BankAccounts
            .Where(a => a.BankConnectionId == bankId);

        var transactions = _db.Transactions
            .Where(t => t.BankAccount.BankConnectionId == bankId);

        _db.BankAccounts.RemoveRange(accounts);
        _db.Transactions.RemoveRange(transactions);
        _db.BankConnections.Remove(bank);

        await _db.SaveChangesAsync();

        return Ok(new { message = "bank removed" });
    }



    // =========================
    // SYNC SINGLE BANK
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
            var result = await SyncBankAsync(bank);

            results.Add(new
            {
                bank.Id,
                bank.InstitutionName,
                result
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
    // FULL SYNC (ACCOUNTS + TRANSACTIONS)
    // =========================
    private async Task<object> SyncBankAsync(BankConnection bank)
    {
        // =========================
        // 1. GET PLAID ACCOUNTS
        // =========================
        var accountsRes = await _plaid.GetAccountsAsync(bank.AccessToken);

        var existingAccounts = await _db.BankAccounts
            .Where(a => a.BankConnectionId == bank.Id)
            .ToListAsync();

        var existingMap = existingAccounts
            .ToDictionary(x => x.PlaidAccountId, x => x);

        // =========================
        // UPSERT ACCOUNTS
        // =========================
        foreach (var acc in accountsRes.accounts)
        {
            if (existingMap.TryGetValue(acc.account_id, out var existing))
            {
                existing.Name = acc.name;
                existing.Type = acc.type;
                existing.Subtype = acc.subtype;
                existing.Balance = acc.balances?.current;
            }
            else
            {
                _db.BankAccounts.Add(new BankAccount
                {
                    PlaidAccountId = acc.account_id,
                    Name = acc.name,
                    Type = acc.type,
                    Subtype = acc.subtype,
                    Balance = acc.balances?.current,
                    BankConnectionId = bank.Id
                });
            }
        }

        // =========================
        // REMOVE MISSING ACCOUNTS
        // =========================
        var plaidAccountIds = accountsRes.accounts
            .Select(a => a.account_id)
            .ToHashSet();

        var toRemove = existingAccounts
            .Where(a => !plaidAccountIds.Contains(a.PlaidAccountId))
            .ToList();

        _db.BankAccounts.RemoveRange(toRemove);

        await _db.SaveChangesAsync();

        // =========================
        // RELOAD ACCOUNT MAP
        // =========================
        var accountsMap = await _db.BankAccounts
            .Where(a => a.BankConnectionId == bank.Id)
            .ToDictionaryAsync(x => x.PlaidAccountId, x => x.Id);

        // =========================
        // SYNC TRANSACTIONS
        // =========================
        var txResult = await SyncTransactionsAsync(bank, accountsMap);

        bank.TransactionsCursor = txResult.cursor;
        await _db.SaveChangesAsync();

        return new
        {
            accounts = accountsMap.Count,
            txResult.added,
            txResult.modified,
            txResult.removed
        };
    }

    // =========================
    // TRANSACTION SYNC (OPTIMIZED)
    // =========================
    private async Task<(int added, int modified, int removed, string cursor)>
        SyncTransactionsAsync(BankConnection bank, Dictionary<string, int> accountsMap)
    {
        int added = 0, modified = 0, removed = 0;

        string cursor = bank.TransactionsCursor ?? "";
        bool hasMore = true;

        var txMap = await _db.Transactions
            .Where(x => x.BankAccount.BankConnectionId == bank.Id)
            .ToDictionaryAsync(x => x.PlaidTransactionId);

        var newTransactions = new List<Transaction>();

        while (hasMore)
        {
            var res = await _plaid.PostSyncAsync(cursor, bank.AccessToken);

            // ADD
            foreach (var t in res.added)
            {
                if (txMap.ContainsKey(t.transaction_id))
                    continue;

                if (!accountsMap.TryGetValue(t.account_id, out var accountId))
                    continue;

                var entity = new Transaction
                {
                    PlaidTransactionId = t.transaction_id,
                    Amount = t.amount,
                    Name = t.name,
                    Category = t.category != null ? string.Join(",", t.category) : null,
                    Date = DateTime.ParseExact(t.date, "yyyy-MM-dd", null),
                    BankAccountId = accountId
                };

                newTransactions.Add(entity);
                txMap[t.transaction_id] = entity;

                added++;
            }

            // MODIFY
            foreach (var t in res.modified)
            {
                if (txMap.TryGetValue(t.transaction_id, out var existing))
                {
                    existing.Amount = t.amount;
                    existing.Name = t.name;
                    existing.Category = t.category != null ? string.Join(",", t.category) : null;
                    existing.Date = DateTime.ParseExact(t.date, "yyyy-MM-dd", null);

                    modified++;
                }
            }

            // REMOVE
            foreach (var t in res.removed)
            {
                if (txMap.TryGetValue(t.transaction_id, out var existing))
                {
                    _db.Transactions.Remove(existing);
                    txMap.Remove(t.transaction_id);
                    removed++;
                }
            }

            cursor = res.next_cursor;
            hasMore = res.has_more;
        }

        if (newTransactions.Count > 0)
            _db.Transactions.AddRange(newTransactions);

        await _db.SaveChangesAsync();

        return (added, modified, removed, cursor);
    }

}