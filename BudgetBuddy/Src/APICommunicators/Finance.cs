using System.Text.Json;

namespace BudgetBuddy;

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
    public async Task<List<BankConnection>?> GetAllBanksAsync()
    {
        var res = await _client.GetAsync(Endpoint("/GetAllBanks"));

        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<BankConnection>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    // -----------------------------
    // GET ALL TRANSACTIONS
    // -----------------------------
    public async Task<List<Transaction>?> GetAllTransactionsAsync()
    {
        var res = await _client.GetAsync(Endpoint("/GetAllTransactions"));

        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<Transaction>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
}