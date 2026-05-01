//using Microsoft.AspNetCore.Mvc;
//using System.Text;
//using System.Text.Json;

//namespace BudgetBuddyAPI.Controllers;

//[ApiController]
//[Route("api/plaid-debug")]
//public class PlaidDebugController : ControllerBase
//{
//    private readonly HttpClient _http;
//    private readonly string _clientId;
//    private readonly string _secret;

//    public PlaidDebugController(IHttpClientFactory factory, IConfiguration config)
//    {
//        _http = factory.CreateClient();
//        _http.BaseAddress = new Uri("https://sandbox.plaid.com");

//        _clientId = config["Plaid:ClientId"]!;
//        _secret = config["Plaid:Secret"]!;
//    }

//    private async Task<IActionResult> PostRaw(string endpoint, object body)
//    {
//        var payload = new Dictionary<string, object>
//        {
//            { "client_id", _clientId },
//            { "secret", _secret }
//        };

//        // merge incoming body
//        foreach (var prop in body.GetType().GetProperties())
//        {
//            payload[prop.Name] = prop.GetValue(body)!;
//        }

//        var json = JsonSerializer.Serialize(payload);
//        var content = new StringContent(json, Encoding.UTF8, "application/json");

//        var res = await _http.PostAsync(endpoint, content);
//        var raw = await res.Content.ReadAsStringAsync();

//        return StatusCode((int)res.StatusCode, raw);
//    }

//    // =========================
//    // ACCOUNTS
//    // =========================
//    [HttpGet("accounts")]
//    public async Task<IActionResult> GetAccounts([FromQuery] string accessToken)
//    {
//        return await PostRaw("/accounts/get", new
//        {
//            access_token = accessToken
//        });
//    }

//    // =========================
//    // ITEM
//    // =========================
//    [HttpGet("item")]
//    public async Task<IActionResult> GetItem([FromQuery] string accessToken)
//    {
//        return await PostRaw("/item/get", new
//        {
//            access_token = accessToken
//        });
//    }

//    // =========================
//    // INSTITUTION
//    // =========================
//    [HttpGet("institution")]
//    public async Task<IActionResult> GetInstitution([FromQuery] string institutionId)
//    {
//        return await PostRaw("/institutions/get_by_id", new
//        {
//            institution_id = institutionId,
//            country_codes = new[] { "CA" }
//        });
//    }

//    // =========================
//    // TRANSACTIONS SYNC
//    // =========================
//    [HttpGet("sync")]
//    public async Task<IActionResult> Sync(
//        [FromQuery] string accessToken,
//        [FromQuery] string? cursor)
//    {
//        return await PostRaw("/transactions/sync", new
//        {
//            access_token = accessToken,
//            cursor = cursor ?? "",
//            count = 500
//        });
//    }


//}