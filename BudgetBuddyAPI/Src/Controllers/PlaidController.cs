using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;

namespace BudgetBuddyAPI;

[Authorize]
[ApiController]
[Route("api/plaid")]
public class PlaidController : BaseController
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
        var token = await _plaid.CreateLinkTokenAsync(UserId.ToString());
        return Ok(new { link_token = token });
    }

    // =========================
    // EXCHANGE TOKEN
    // =========================
    [HttpPost("[action]")]
    public async Task<IActionResult> Exchange([FromBody] ExchangeRequest req)
    {
        var userId = UserId;

        var (accessToken, itemId) = await _plaid.ExchangePublicTokenAsync(req.public_token);

        var itemRes = await _plaid.PostAsync("/item/get", new { access_token = accessToken });
        var plaidInstitutionId = itemRes.GetProperty("item")
                                        .GetProperty("institution_id")
                                        .GetString()!;

        var institution = await _db.Institutions
            .FirstOrDefaultAsync(i => i.institution_id == plaidInstitutionId);

        if (institution == null)
        {
            var instRes = await _plaid.GetInstitutionAsync(plaidInstitutionId);
            instRes.id = 0;
            instRes.updated_at = DateTime.UtcNow;

            _db.Institutions.Add(instRes);
            await _db.SaveChangesAsync();

            institution = instRes;
        }

        var existing = await _db.BankConnections
            .FirstOrDefaultAsync(x => x.item_id == itemId && x.user_id == userId);

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
            user_id = userId,
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
        var userId = UserId;

        var bank = await _db.BankConnections
            .FirstOrDefaultAsync(x => x.id == bankId && x.user_id == userId);

        if (bank == null) return NotFound("Bank not found");

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
        var userId = UserId;

        var bank = await _db.BankConnections
            .FirstOrDefaultAsync(x => x.id == bankId && x.user_id == userId);

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
        var userId = UserId;

        var banks = await _db.BankConnections
            .Where(b => b.user_id == userId)
            .ToListAsync();

        var results = new List<object>();

        foreach (var bank in banks)
        {
            var result = await SyncBankAsync(bank);
            results.Add(new { bank.id, bank.institution_id, result });
        }

        return Ok(new { message = "All banks synced", count = banks.Count, results });
    }

    // =========================
    // FULL SYNC
    // =========================
    private async Task<object> SyncBankAsync(BankConnection bank)
    {
        var plaidAccounts = await _plaid.GetAccountsAsync(bank.access_token);

        var existingAccounts = await _db.BankAccounts
            .Where(a => a.bank_connection_id == bank.id)
            .ToListAsync();

        var existingMap = existingAccounts.ToDictionary(a => a.account_id);
        var plaidIds = plaidAccounts.Select(a => a.account_id).ToHashSet();

        var toRemove = existingAccounts
            .Where(a => !plaidIds.Contains(a.account_id))
            .ToList();

        _db.BankAccounts.RemoveRange(toRemove);

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
                acc.id = 0;
                acc.bank_connection_id = bank.id;
                _db.BankAccounts.Add(acc);
            }
        }

        await _db.SaveChangesAsync();

        var accountsMap = await _db.BankAccounts
            .Where(a => a.bank_connection_id == bank.id)
            .Select(a => new
            {
                a.account_id,
                a.id,
                a.type
            })
            .ToDictionaryAsync(
                a => a.account_id,
                a => new AccountMapItem
                {
                    id = a.id,
                    type = a.type
                });

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
        SyncTransactionsAsync(
            BankConnection bank,
            Dictionary<string, AccountMapItem> accountsMap)
    {
        int added = 0, modified = 0, removed = 0;

        var txMap = await _db.Transactions
            .Include(t => t.counterparties)
            .Include(t => t.location)
            .Include(t => t.payment_meta)
            .Where(t => t.bank_account.bank_connection_id == bank.id)
            .ToDictionaryAsync(t => t.transaction_id);

        var toAdd = new List<Transaction>();

        var (plaidAdded, plaidModified, plaidRemoved, cursor, hasMore) =
            await _plaid.SyncTransactionsAsync(
                bank.access_token,
                bank.transactions_cursor);

        while (true)
        {
            // =========================
            // ADD
            // =========================
            foreach (var t in plaidAdded)
            {
                if (txMap.ContainsKey(t.transaction_id))
                    continue;

                if (!accountsMap.TryGetValue(t.account_id, out var account))
                    continue;

                var entity = new Transaction
                {
                    transaction_id = t.transaction_id,
                    account_id = t.account_id,

                    amount = NormalizeAmount(
                        (decimal)t.amount,
                        account.type),

                    name = t.name,
                    merchant_name = t.merchant_name,
                    merchant_entity_id = t.merchant_entity_id,
                    iso_currency_code = t.iso_currency_code,
                    unofficial_currency_code = t.unofficial_currency_code,

                    date = DateFix.ToUtc(t.date),
                    authorized_date = DateFix.ToUtc(t.authorized_date),
                    datetime = DateFix.ToUtc(t.datetime),
                    authorized_datetime = DateFix.ToUtc(t.authorized_datetime),

                    pending = t.pending,
                    pending_transaction_id = t.pending_transaction_id,
                    payment_channel = t.payment_channel,
                    transaction_type = t.transaction_type,
                    transaction_code = t.transaction_code,
                    logo_url = t.logo_url,
                    website = t.website,
                    personal_finance_category_icon_url = t.personal_finance_category_icon_url,
                    account_owner = t.account_owner,
                    check_number = t.check_number,
                    personal_finance_category = t.personal_finance_category,

                    bank_account_id = account.id,

                    counterparties = t.counterparties?.Select(c =>
                        new TransactionCounterparty
                        {
                            name = c.name,
                            type = c.type,
                            logo_url = c.logo_url,
                            website = c.website,
                            confidence_level = c.confidence_level,
                            entity_id = c.entity_id
                        }).ToList() ?? new(),

                    location = t.location != null
                        ? new TransactionLocation
                        {
                            address = t.location.address,
                            city = t.location.city,
                            region = t.location.region,
                            country = t.location.country,
                            postal_code = t.location.postal_code,
                            lat = t.location.lat,
                            lon = t.location.lon,
                            store_number = t.location.store_number
                        }
                        : null,

                    payment_meta = t.payment_meta != null
                        ? new TransactionPaymentMeta
                        {
                            by_order_of = t.payment_meta.by_order_of,
                            payee = t.payment_meta.payee,
                            payer = t.payment_meta.payer,
                            payment_method = t.payment_meta.payment_method,
                            payment_processor = t.payment_meta.payment_processor,
                            ppd_id = t.payment_meta.ppd_id,
                            reason = t.payment_meta.reason,
                            reference_number = t.payment_meta.reference_number
                        }
                        : null
                };

                toAdd.Add(entity);
                txMap[t.transaction_id] = entity;

                added++;
            }

            // =========================
            // MODIFY
            // =========================
            foreach (var t in plaidModified)
            {
                if (!txMap.TryGetValue(t.transaction_id, out var existing))
                    continue;

                if (accountsMap.TryGetValue(t.account_id, out var account))
                {
                    existing.amount = NormalizeAmount(
                        (decimal)t.amount,
                        account.type);
                }

                existing.name = t.name;
                existing.merchant_name = t.merchant_name;
                existing.merchant_entity_id = t.merchant_entity_id;
                existing.unofficial_currency_code = t.unofficial_currency_code;

                existing.date = DateFix.ToUtc(t.date);
                existing.authorized_date = DateFix.ToUtc(t.authorized_date);
                existing.datetime = DateFix.ToUtc(t.datetime);
                existing.authorized_datetime = DateFix.ToUtc(t.authorized_datetime);

                existing.pending = t.pending;
                existing.pending_transaction_id = t.pending_transaction_id;
                existing.payment_channel = t.payment_channel;
                existing.transaction_type = t.transaction_type;
                existing.transaction_code = t.transaction_code;
                existing.logo_url = t.logo_url;
                existing.website = t.website;
                existing.personal_finance_category_icon_url = t.personal_finance_category_icon_url;
                existing.account_owner = t.account_owner;
                existing.check_number = t.check_number;
                existing.personal_finance_category = t.personal_finance_category;

                modified++;
            }

            // =========================
            // REMOVE
            // =========================
            foreach (var txId in plaidRemoved)
            {
                if (!txMap.TryGetValue(txId, out var existing))
                    continue;

                _db.Transactions.Remove(existing);
                txMap.Remove(txId);

                removed++;
            }

            if (!hasMore)
                break;

            (plaidAdded, plaidModified, plaidRemoved, cursor, hasMore) =
                await _plaid.SyncTransactionsAsync(
                    bank.access_token,
                    cursor);
        }

        if (toAdd.Count > 0)
            _db.Transactions.AddRange(toAdd);

        await _db.SaveChangesAsync();

        return (added, modified, removed, cursor);
    }

    private decimal NormalizeAmount(decimal amount, string? accountType)
    {
        accountType = accountType?.ToLowerInvariant();

        if (accountType == "credit" || accountType == "loan")
        {
            return -amount;
        }

        return amount;
    }
}
   


public record ExchangeRequest(string public_token);

public class AccountMapItem
{
    public int id { get; set; }
    public string? type { get; set; }
}