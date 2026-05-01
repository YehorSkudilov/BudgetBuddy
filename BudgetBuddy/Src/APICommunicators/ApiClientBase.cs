using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BudgetBuddy;

public abstract class ApiClientBase
{
    protected static readonly HttpClient SharedClient = new HttpClient();
    protected readonly HttpClient _client;

    protected ApiClientBase()
    {
        _client = SharedClient;
    }

    protected async Task<T?> ReadJsonAsync<T>(HttpResponseMessage res)
    {
        var json = await res.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON deserialization failed: {ex}");
            throw;
        }
    }

    protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        // attach token
        if (!string.IsNullOrEmpty(AuthStore.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthStore.Token);
        }

        HttpResponseMessage response;

        try
        {
            response = await _client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            // network failure (no internet / DNS / server down)
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                ReasonPhrase = $"Network error: {ex.Message}"
            };
        }

        // handle unauthorized globally
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            AuthStore.Logout();
            return response; // let UI react instead of crashing
        }

        return response; // ❗ NO EnsureSuccessStatusCode()
    }

    protected string BuildUrl(string path)
        => $"{ApiCommunicators.BaseUrl}{path}";
}