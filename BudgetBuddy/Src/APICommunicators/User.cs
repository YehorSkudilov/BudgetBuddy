using System.Text;
using System.Text.Json;

namespace BudgetBuddy;

public class User : ApiClientBase
{
    private string Endpoint(string path)
        => BuildUrl($"/api/user{path}");

    private StringContent JsonContent(object obj)
        => new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            Endpoint("/Login"))
        {
            Content = JsonContent(new { email, password })
        });

        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            throw new Exception(error);
        }

        return await ReadJsonAsync<AuthResponse>(res);
    }

    public async Task<AuthResponse?> RegisterAsync(string email, string username, string password)
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            Endpoint("/Register"))
        {
            Content = JsonContent(new { email, username, password })
        });

        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            throw new Exception(error);
        }

        return await ReadJsonAsync<AuthResponse>(res);
    }
    public async Task<User?> GetUserAsync()
    {
        var res = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            Endpoint("/GetUser")
        ));

        if (!res.IsSuccessStatusCode)
            throw new Exception(await res.Content.ReadAsStringAsync());

        return await ReadJsonAsync<User>(res);
    }
}