using System.Diagnostics;
using System.Text.Json;

namespace BudgetBuddy;

public partial class BanksPage : ContentView
{
    private bool isWebViewOpen;

    public bool IsWebViewOpen
    {
        get => isWebViewOpen;
        set
        {
            if (isWebViewOpen == value) return;

            isWebViewOpen = value;
            OnPropertyChanged(nameof(IsWebViewOpen));
            OnIsWebViewOpenChanged(value);
        }
    }

    public BanksPage()
    {
        InitializeComponent();

        MainGrid.BindingContext = this;

        XWebView.Navigating += XWebView_Navigating;
    }

    // -----------------------------
    // START FLOW
    // -----------------------------
    private async void Add_Clicked(object sender, EventArgs e)
    {
        IsWebViewOpen = true;

        var token = await CreateLinkToken();

        XWebView.Source = "http://10.100.100.135:5107/plaid.html";

        XWebView.Navigated += async (s, args) =>
        {
            await Task.Delay(800);

            await XWebView.EvaluateJavaScriptAsync($@"
                (function waitForPlaid() {{
                    if (typeof startPlaid === 'function') {{
                        startPlaid('{token}');
                    }} else {{
                        setTimeout(waitForPlaid, 100);
                    }}
                }})();
            ");
        };
    }

    // -----------------------------
    // CREATE LINK TOKEN
    // -----------------------------
    private async Task<string> CreateLinkToken()
    {
        using var client = new HttpClient();

        var res = await client.PostAsync(
            "http://10.100.100.135:5107/api/plaid/CreateLinkToken",
            null);

        var json = await res.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("link_token").GetString();
    }

    // -----------------------------
    // HANDLE WEBVIEW CALLBACKS
    // -----------------------------
    private async void XWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Url))
            return;

        if (e.Url.StartsWith("maui://plaid-success") ||
            e.Url.StartsWith("maui://plaid-exit"))
        {
            e.Cancel = true;

            try
            {
                var uri = new Uri(e.Url);

                var data = System.Web.HttpUtility
                    .ParseQueryString(uri.Query)
                    .Get("data");

                if (!string.IsNullOrEmpty(data))
                {
                    var json = Uri.UnescapeDataString(data);
                    Debug.WriteLine("PLAID CALLBACK: " + json);

                    if (e.Url.StartsWith("maui://plaid-success"))
                    {
                        using var doc = JsonDocument.Parse(json);

                        if (doc.RootElement.TryGetProperty("public_token", out var token))
                        {
                            var publicToken = token.GetString();

                            Debug.WriteLine("PUBLIC TOKEN: " + publicToken);

                            await ExchangePublicToken(publicToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Plaid parse error: " + ex.Message);

            }

            IsWebViewOpen = false;
        }
    }

    // -----------------------------
    // EXCHANGE PUBLIC TOKEN
    // -----------------------------
    private async Task ExchangePublicToken(string publicToken)
    {
        try
        {
            using var client = new HttpClient();

            var payload = new
            {
                public_token = publicToken
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var res = await client.PostAsync(
                "http://10.100.100.135:5107/api/plaid/Exchange",
                jsonContent
            );

            var response = await res.Content.ReadAsStringAsync();

            Debug.WriteLine("EXCHANGE RESPONSE: " + response);

          
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Exchange error: " + ex.Message);


        }
    }

    // -----------------------------
    // WEBVIEW STATE CONTROL
    // -----------------------------
    private void OnIsWebViewOpenChanged(bool isOpen)
    {
        if (!isOpen)
        {
            XWebView.Source = "about:blank";
        }
    }
}