using System.Text.Json;

namespace BudgetBuddy;

public class Finance : ApiClientBase
{
    private string Endpoint(string path)
        => BuildUrl($"/api/finance{path}");

    public async Task<List<BankConnection>?> GetAllBanksAsync()
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            Endpoint("/GetAllBanks")
        ));

        return await ReadJsonAsync<List<BankConnection>>(res);
    }

    public async Task<List<Transaction>?> GetAllTransactionsAsync()
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            Endpoint("/GetAllTransactions")
        ));

        return await ReadJsonAsync<List<Transaction>>(res);
    }
}