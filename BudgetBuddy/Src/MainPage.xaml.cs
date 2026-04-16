using System.Net.Http.Json;

namespace BudgetBuddy;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("http://localhost:5107")
    };

    private string _publicToken = "";
    private string _accessToken = "";

    public MainPage()
    {
        InitializeComponent();

        PlaidWebView.Navigated += OnWebViewNavigated;

        // ✅ ADD THIS HERE
        PlaidWebView.Navigating += (s, e) =>
        {
            var url = e.Url?.ToLower() ?? "";

            if (url.Contains("localhost/success") || url.Contains("localhost/exit"))
            {
                e.Cancel = true;
            }
        };
    }

    private async void OnPlaidClicked(object sender, EventArgs e)
    {
        try
        {
            PlaidBtn.IsEnabled = false;
            ResultLabel.Text = "Loading Plaid...";

            var url = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5107/plaid.html"
                : "http://localhost:5107/plaid.html";

            PlaidWebView.IsVisible = true;
            PlaidWebView.Source = new UrlWebViewSource { Url = url };

            ResultLabel.Text = "Waiting for bank login...";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
        finally
        {
            PlaidBtn.IsEnabled = true;
        }
    }

    private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        var url = e.Url?.ToLower() ?? "";

        // SUCCESS
        if (url.Contains("localhost/success?token="))
        {
            _publicToken = url.Split("token=")[1];

            PlaidWebView.IsVisible = false;
            PlaidWebView.Source = "about:blank";

            await ExchangeToken();
            return;
        }

        // EXIT
        if (url.Contains("localhost/exit"))
        {
            PlaidWebView.IsVisible = false;
            ResultLabel.Text = "Bank connection cancelled";
            return;
        }
    }

    private async Task ExchangeToken()
    {
        try
        {
            var res = await _http.PostAsJsonAsync(
                "/api/plaid/exchange",
                new { public_token = _publicToken }
            );

            var json = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            if (!json.TryGetValue("access_token", out var tokenObj))
            {
                ResultLabel.Text = "Failed to exchange token";
                return;
            }

            _accessToken = tokenObj?.ToString();

            await LoadTransactions();
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }

    // ✅ REAL MODEL
    public class Transaction
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
    }

    private async Task LoadTransactions()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<List<Transaction>>(
                $"/api/plaid/transactions?accessToken={_accessToken}"
            );

            TransactionsView.ItemsSource = res;

            ResultLabel.Text = "Bank connected!";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }
}