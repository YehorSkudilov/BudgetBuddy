using System.Text.Json;

namespace BudgetBuddy.Services;

public class Finance
{
    private readonly HttpClient _client;

    private string Endpoint(string path)
        => $"{ApiCommunicators.BaseUrl}/api/finance{path}";

    public Finance()
    {
        _client = new HttpClient();
    }

    // -----------------------------
    // GET ALL BANKS + ACCOUNTS
    // -----------------------------
    public async Task<List<Bank>?> GetAllBanksAsync()
    {
        var res = await _client.GetAsync(Endpoint("/GetAllBanks"));

        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<Bank>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
}