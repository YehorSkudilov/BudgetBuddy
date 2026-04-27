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
        var token = await _plaid.CreateLinkTokenAsync(Guid.NewGuid().ToString());
        return Ok(new { link_token = token });
    }

    // =========================
    // EXCHANGE TOKEN
    // =========================
    [HttpPost("[action]")]
    public async Task<IActionResult> Exchange([FromBody] ExchangeRequest req)
    {
        var (accessToken, itemId) = await _plaid.ExchangePublicTokenAsync(req.public_token);

        // Get institution info from item
        var itemRes = await _plaid.PostAsync("/item/get", new { access_token = accessToken });
        var plaidInstitutionId = itemRes.GetProperty("item")
                                        .GetProperty("institution_id")
                                        .GetString()!;

        // Upsert institution
        var institution = await _db.Institutions
            .FirstOrDefaultAsync(i => i.institution_id == plaidInstitutionId);

        if (institution == null)
        {
            var instRes = await _plaid.GetInstitutionAsync(plaidInstitutionId);
            instRes.id = 0; // clear Plaid's id field if any, let EF assign PK
            institution = instRes;
            institution.updated_at = DateTime.UtcNow;
            _db.Institutions.Add(institution);
            await _db.SaveChangesAsync();
        }

        // Prevent duplicate connections
        var existing = await _db.BankConnections
            .FirstOrDefaultAsync(x => x.item_id == itemId);

        if (existing != null)
        {
            existing.access_token = accessToken;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Bank already connected, token refreshed", existing.id });
        }

        var connection = new BankConnection
        {
            access_token = accessToken,
            item_id = itemId,
            institution_id = institution.id,
            user_id = req.user_id,
            created_at = DateTime.UtcNow
        };

        _db.BankConnections.Add(connection);
        await _db.SaveChangesAsync();

        return Ok(new { connection.id, institution.name });
    }

    // =========================
    // REMOVE BANK
    // =========================
    [HttpDelete("[action]/{bankId}")]
    public async Task<IActionResult> RemoveBank(int bankId)
    {
        var bank = await _db.BankConnections.FirstOrDefaultAsync(x => x.id == bankId);
        if (bank == null) return NotFound("Bank not found");

        // Cascade handles accounts + transactions via EF config
        _db.BankConnections.Remove(bank);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Bank removed" });
    }

    // =========================
    // SYNC SINGLE BANK
    // =========================
    [HttpGet("[action]/{bankId}")]
    public async Task<IActionResult> SyncBank(int bankId)
    {
        var bank = await _db.BankConnections.FirstOrDefaultAsync(x => x.id == bankId);
        if (bank == null) return NotFound("Bank not found");

        var result = await SyncBankAsync(bank);
        return Ok(new { message = "synced", bankId, result });
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
            results.Add(new { bank.id, bank.institution_id, result });
        }

        return Ok(new { message = "All banks synced", count = banks.Count, results });
    }

    // =========================
    // FULL SYNC (ACCOUNTS + TRANSACTIONS)
    // =========================
    private async Task<object> SyncBankAsync(BankConnection bank)
    {
        // ---- ACCOUNTS ----
        var plaidAccounts = await _plaid.GetAccountsAsync(bank.access_token);

        var existingAccounts = await _db.BankAccounts
            .Where(a => a.bank_connection_id == bank.id)
            .ToListAsync();

        var existingMap = existingAccounts.ToDictionary(a => a.account_id);
        var plaidIds = plaidAccounts.Select(a => a.account_id).ToHashSet();

        // Remove closed/removed accounts
        var toRemove = existingAccounts.Where(a => !plaidIds.Contains(a.account_id)).ToList();
        _db.BankAccounts.RemoveRange(toRemove);

        // Upsert accounts
        foreach (var acc in plaidAccounts)
        {
            if (existingMap.TryGetValue(acc.account_id, out var existing))
            {
                existing.name = acc.name;
                existing.official_name = acc.official_name;
                existing.type = acc.type;
                existing.subtype = acc.subtype;
                existing.mask = acc.mask;
                existing.holder_category = acc.holder_category;
                existing.balances = acc.balances;
            }
            else
            {
                acc.id = 0; // let EF assign PK
                acc.bank_connection_id = bank.id;
                _db.BankAccounts.Add(acc);
            }
        }

        await _db.SaveChangesAsync();

        // Reload account map for transaction linking
        var accountsMap = await _db.BankAccounts
            .Where(a => a.bank_connection_id == bank.id)
            .ToDictionaryAsync(a => a.account_id, a => a.id);

        // ---- TRANSACTIONS ----
        var txResult = await SyncTransactionsAsync(bank, accountsMap);

        bank.transactions_cursor = txResult.cursor;
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
    // TRANSACTION SYNC
    // =========================
    private async Task<(int added, int modified, int removed, string cursor)>
        SyncTransactionsAsync(BankConnection bank, Dictionary<string, int> accountsMap)
    {
        int added = 0, modified = 0, removed = 0;

        var txMap = await _db.Transactions
            .Where(t => t.bank_account.bank_connection_id == bank.id)
            .ToDictionaryAsync(t => t.transaction_id);

        var toAdd = new List<Transaction>();

        var (plaidAdded, plaidModified, plaidRemoved, cursor, hasMore) =
            await _plaid.SyncTransactionsAsync(bank.access_token, bank.transactions_cursor);

        while (true)
        {
            // ADD
            foreach (var t in plaidAdded)
            {
                if (txMap.ContainsKey(t.transaction_id)) continue;
                if (!accountsMap.TryGetValue(t.account_id, out var accountId)) continue;

                t.id = 0;
                t.bank_account_id = accountId;
                toAdd.Add(t);
                txMap[t.transaction_id] = t;
                added++;
            }

            // MODIFY
            foreach (var t in plaidModified)
            {
                if (!txMap.TryGetValue(t.transaction_id, out var existing)) continue;

                existing.amount = t.amount;
                existing.name = t.name;
                existing.merchant_name = t.merchant_name;
                existing.date = t.date;
                existing.authorized_date = t.authorized_date;
                existing.pending = t.pending;
                existing.payment_channel = t.payment_channel;
                existing.logo_url = t.logo_url;
                existing.website = t.website;
                existing.personal_finance_category = t.personal_finance_category;
                existing.counterparties = t.counterparties;
                modified++;
            }

            // REMOVE
            foreach (var txId in plaidRemoved)
            {
                if (!txMap.TryGetValue(txId, out var existing)) continue;
                _db.Transactions.Remove(existing);
                txMap.Remove(txId);
                removed++;
            }

            if (!hasMore) break;

            (plaidAdded, plaidModified, plaidRemoved, cursor, hasMore) =
                await _plaid.SyncTransactionsAsync(bank.access_token, cursor);
        }

        if (toAdd.Count > 0)
            _db.Transactions.AddRange(toAdd);

        await _db.SaveChangesAsync();

        return (added, modified, removed, cursor);
    }
}

public record ExchangeRequest(string public_token, string user_id);