using System.Text;
using System.Text.Json;

namespace BudgetBuddy;

public class Plaid : ApiClientBase
{
    private string Endpoint(string path)
        => BuildUrl($"/api/plaid{path}");

    public async Task<string?> CreateLinkTokenAsync()
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            Endpoint("/CreateLinkToken")
        ));

        var json = await ReadJsonAsync<JsonElement>(res);

        return json.ValueKind == JsonValueKind.Object &&
               json.TryGetProperty("link_token", out var token)
            ? token.GetString()
            : null;
    }

    public async Task<string?> ExchangePublicTokenAsync(string publicToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            Endpoint("/Exchange")
        )
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { public_token = publicToken }),
                Encoding.UTF8,
                "application/json"
            )
        };

        var res = await SendAsync(request);
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string?> SyncAllBanksAsync()
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            Endpoint("/SyncAllBanks")
        ));

        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string?> RemoveBankAsync(int bankId)
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Delete,
            Endpoint($"/RemoveBank/{bankId}")
        ));

        return await res.Content.ReadAsStringAsync();
    }
}