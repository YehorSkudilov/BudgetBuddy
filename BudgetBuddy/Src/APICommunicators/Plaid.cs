using System.Text;
using System.Text.Json;

namespace BudgetBuddy;

public class Plaid
{
    private readonly HttpClient _client;

    private string Endpoint(string path) => $"{ApiCommunicators.BaseUrl}/api/plaid{path}";

    public Plaid()
    {
        _client = new HttpClient();
    }
    // -----------------------------
    // CREATE LINK TOKEN
    // -----------------------------
    public async Task<string?> CreateLinkTokenAsync()
    {
        var res = await _client.PostAsync(Endpoint("/CreateLinkToken"), null);

        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("link_token").GetString();
    }

    // -----------------------------
    // EXCHANGE PUBLIC TOKEN
    // -----------------------------
    public async Task<string> ExchangePublicTokenAsync(string publicToken)
    {
        var payload = new
        {
            public_token = publicToken
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var res = await _client.PostAsync(
            Endpoint("/Exchange"),
            jsonContent
        );

        res.EnsureSuccessStatusCode();

        return await res.Content.ReadAsStringAsync();
    }
}