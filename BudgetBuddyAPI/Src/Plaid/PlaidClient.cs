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
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public PlaidClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _clientId = config["Plaid:ClientId"];
        _secret = config["Plaid:Secret"];

        _http.BaseAddress = new Uri("https://sandbox.plaid.com");
    }

    // =========================
    // CORE POST (FIXED)
    // =========================
    private async Task<TResponse> PostAsync<TResponse>(string url, object body)
    {
        var basePayload = new Dictionary<string, object?>
        {
            ["client_id"] = _clientId,
            ["secret"] = _secret
        };

        // serialize body properly (keeps nested objects intact)
        var bodyJson = JsonSerializer.Serialize(body, JsonOptions);
        var bodyDict = JsonSerializer.Deserialize<Dictionary<string, object>>(bodyJson);

        if (bodyDict != null)
        {
            foreach (var kv in bodyDict)
                basePayload[kv.Key] = kv.Value;
        }

        var finalJson = JsonSerializer.Serialize(basePayload, JsonOptions);

        var response = await _http.PostAsync(
            url,
            new StringContent(finalJson, Encoding.UTF8, "application/json")
        );

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Plaid error ({url}): {content}");

        return JsonSerializer.Deserialize<TResponse>(content, JsonOptions)!;
    }
    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    public Task<LinkTokenResponse> CreateLinkTokenAsync(LinkTokenRequest req)
        => PostAsync<LinkTokenResponse>("/link/token/create", req);

    public Task<ExchangeTokenResponse> ExchangePublicTokenAsync(ExchangeTokenRequest req)
        => PostAsync<ExchangeTokenResponse>("/item/public_token/exchange", req);

    public Task<PlaidItemResponse> GetItemAsync(string accessToken)
        => PostAsync<PlaidItemResponse>("/item/get", new { access_token = accessToken });

    public Task<PlaidInstitutionResponse> GetInstitutionAsync(string institutionId)
        => PostAsync<PlaidInstitutionResponse>("/institutions/get_by_id",
            new
            {
                institution_id = institutionId,
                country_codes = new[] { "CA" }
            });

    public Task<PlaidSyncResponse> PostSyncAsync(string cursor, string accessToken)
        => PostAsync<PlaidSyncResponse>("/transactions/sync",
            new
            {
                access_token = accessToken,
                cursor = cursor ?? "",
                count = 500
            });

    public Task<TransactionsResponse> GetTransactionsAsync(TransactionsRequest req)
        => PostAsync<TransactionsResponse>("/transactions/get", req);
}