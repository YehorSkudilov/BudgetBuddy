using System.Net.Http;
using System.Text;
using System.Text.Json;
using BudgetBuddyAPI.Src.DB;

namespace BudgetBuddyAPI;

public class PlaidClient
{
    private readonly HttpClient _http;
    private readonly string _clientId;
    private readonly string _secret;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public PlaidClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _clientId = config["Plaid:ClientId"]!;
        _secret = config["Plaid:Secret"]!;
        _http.BaseAddress = new Uri(config["Plaid:BaseUrl"]);
    }

    // =========================
    // CORE REQUEST
    // =========================
    public async Task<JsonElement> PostAsync(string url, object body)
    {
        var payload = new Dictionary<string, object?>
        {
            ["client_id"] = _clientId,
            ["secret"] = _secret
        };

        var json = JsonSerializer.Serialize(body, JsonOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
        if (dict != null)
            foreach (var kv in dict)
                payload[kv.Key] = kv.Value;

        var content = JsonSerializer.Serialize(payload, JsonOptions);
        var response = await _http.PostAsync(
            url,
            new StringContent(content, Encoding.UTF8, "application/json")
        );

        var raw = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Plaid error ({url}): {raw}");

        return JsonDocument.Parse(raw).RootElement;
    }

    // =========================
    // LINK TOKEN
    // =========================
    public async Task<string> CreateLinkTokenAsync(string userId)
    {
        var res = await PostAsync("/link/token/create", new
        {
            user = new { client_user_id = userId },
            client_name = "BudgetBuddy",
            products = new[] { "transactions" },
            country_codes = new[] { "CA" },
            language = "en"
        });

        return res.GetProperty("link_token").GetString()!;
    }

    // =========================
    // EXCHANGE TOKEN
    // =========================
    public async Task<(string accessToken, string itemId)> ExchangePublicTokenAsync(string publicToken)
    {
        var res = await PostAsync("/item/public_token/exchange", new
        {
            public_token = publicToken
        });

        return (
            res.GetProperty("access_token").GetString()!,
            res.GetProperty("item_id").GetString()!
        );
    }

    // =========================
    // ACCOUNTS
    // =========================
    public async Task<List<BankAccount>> GetAccountsAsync(string accessToken)
    {
        var res = await PostAsync("/accounts/get", new { access_token = accessToken });

        return res.GetProperty("accounts")
            .Deserialize<List<BankAccount>>(JsonOptions)!;
    }

    // =========================
    // INSTITUTION
    // =========================
    public async Task<Institution> GetInstitutionAsync(string institutionId)
    {
        var res = await PostAsync("/institutions/get_by_id", new
        {
            institution_id = institutionId,
            country_codes = new[] { "CA" }
        });

        return res.GetProperty("institution")
            .Deserialize<Institution>(JsonOptions)!;
    }

    // =========================
    // TRANSACTIONS SYNC
    // =========================
    public async Task<(List<Transaction> added, List<Transaction> modified, List<string> removed, string nextCursor, bool hasMore)>
        SyncTransactionsAsync(string accessToken, string? cursor)
    {
        var res = await PostAsync("/transactions/sync", new
        {
            access_token = accessToken,
            cursor = cursor ?? "",
            count = 500
        });

        var added = res.GetProperty("added").Deserialize<List<Transaction>>(JsonOptions)!;
        var modified = res.GetProperty("modified").Deserialize<List<Transaction>>(JsonOptions)!;
        var removed = res.GetProperty("removed")
            .EnumerateArray()
            .Select(r => r.GetProperty("transaction_id").GetString()!)
            .ToList();

        var nextCursor = res.GetProperty("next_cursor").GetString()!;
        var hasMore = res.GetProperty("has_more").GetBoolean();

        return (added, modified, removed, nextCursor, hasMore);
    }
}
