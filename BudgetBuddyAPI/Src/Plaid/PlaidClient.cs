using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BudgetBuddyAPI;

public class PlaidClient
{
    private readonly HttpClient _http;
    private readonly string _clientId;
    private readonly string _secret;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    public PlaidClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _clientId = config["Plaid:ClientId"]!;
        _secret = config["Plaid:Secret"]!;

        _http.BaseAddress = new Uri("https://sandbox.plaid.com");
    }

    // =========================
    // CORE REQUEST (PUBLIC)
    // =========================
    public async Task<TResponse> PostAsync<TResponse>(string url, object body)
    {
        var payload = new Dictionary<string, object?>
        {
            ["client_id"] = _clientId,
            ["secret"] = _secret
        };

        // safer merge (no nested JSON corruption)
        var json = JsonSerializer.Serialize(body, JsonOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);

        if (dict != null)
        {
            foreach (var kv in dict)
                payload[kv.Key] = kv.Value;
        }

        var contentJson = JsonSerializer.Serialize(payload, JsonOptions);

        var response = await _http.PostAsync(
            url,
            new StringContent(contentJson, Encoding.UTF8, "application/json")
        );

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Plaid error ({url}): {content}");

        return JsonSerializer.Deserialize<TResponse>(content, JsonOptions)!;
    }

    // =========================
    // LINK TOKEN
    // =========================
    public Task<LinkTokenResponse> CreateLinkTokenAsync(LinkTokenRequest req)
        => PostAsync<LinkTokenResponse>("/link/token/create", req);

    // =========================
    // EXCHANGE TOKEN
    // =========================
    public Task<ExchangeTokenResponse> ExchangePublicTokenAsync(ExchangeTokenRequest req)
        => PostAsync<ExchangeTokenResponse>("/item/public_token/exchange", req);
    public Task<PlaidAccountsResponse> GetAccountsAsync(string accessToken)
    => PostAsync<PlaidAccountsResponse>("/accounts/get",
        new
        {
            access_token = accessToken
        });


    // =========================
    // ITEM INFO
    // =========================
    public Task<PlaidItemResponse> GetItemAsync(string accessToken)
        => PostAsync<PlaidItemResponse>("/item/get",
            new { access_token = accessToken });

    // =========================
    // INSTITUTION
    // =========================
    public Task<PlaidInstitutionResponse> GetInstitutionAsync(string institutionId)
        => PostAsync<PlaidInstitutionResponse>("/institutions/get_by_id",
            new
            {
                institution_id = institutionId,
                country_codes = new[] { "CA" }
            });

    // =========================
    // TRANSACTIONS SYNC
    // =========================
    public Task<PlaidSyncResponse> PostSyncAsync(string cursor, string accessToken)
        => PostAsync<PlaidSyncResponse>("/transactions/sync",
            new
            {
                access_token = accessToken,
                cursor = cursor ?? "",
                count = 500
            });

    // optional legacy endpoint
    public Task<TransactionsResponse> GetTransactionsAsync(TransactionsRequest req)
        => PostAsync<TransactionsResponse>("/transactions/get", req);
}