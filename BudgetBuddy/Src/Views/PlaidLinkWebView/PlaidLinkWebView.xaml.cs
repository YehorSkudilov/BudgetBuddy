using System.Diagnostics;
using System.Text.Json;

namespace BudgetBuddy;

public partial class PlaidLinkWebView : Border
{
    private string? _plaidToken;
    private bool _plaidReady;
    private HybridWebView? _webView;

    public event EventHandler<string>? LinkSucceeded;
    public event EventHandler? LinkClosed;

    public PlaidLinkWebView()
    {
        InitializeComponent();
    }

    // ---------------------------------------------------------------
    // OPEN / CLOSE
    // ---------------------------------------------------------------
    public async Task OpenAsync()
    {
        _plaidToken = null;
        _plaidReady = false;

        var tokenTask = Task.Run(() => ApiCommunicators.Plaid.CreateLinkTokenAsync());

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            CreateWebView();
            IsVisible = true;
        });

        _plaidToken = await tokenTask;

        if (string.IsNullOrEmpty(_plaidToken))
        {
            Debug.WriteLine("Failed to get Plaid token");
            await CloseAsync();
            return;
        }

        if (_plaidReady)
            await InjectTokenAsync();
    }

    public async Task CloseAsync()
    {
        _plaidToken = null;
        _plaidReady = false;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsVisible = false;
            DestroyWebView();
        });
    }

    private void CreateWebView()
    {
        DestroyWebView(); // safety — ensure no double-attach

        _webView = new HybridWebView
        {
            DefaultFile = "plaid.html"
        };
        _webView.RawMessageReceived += OnRawMessageReceived;
        Content = _webView;
    }

    private void DestroyWebView()
    {
        if (_webView == null) return;
        _webView.RawMessageReceived -= OnRawMessageReceived;
        Content = null;
        _webView = null;
    }

    // ---------------------------------------------------------------
    // MESSAGE BRIDGE
    // ---------------------------------------------------------------
    private async void OnRawMessageReceived(object? sender, HybridWebViewRawMessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Message)) return;

        Debug.WriteLine($"[PlaidLinkWebView] Message: {e.Message}");

        try
        {
            using var doc = JsonDocument.Parse(e.Message);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "ready": await HandlePlaidReadyAsync(); break;
                case "success": await HandlePlaidSuccessAsync(root); break;
                case "exit": await HandlePlaidExitAsync(root); break;
                default:
                    Debug.WriteLine($"Unknown message type: {type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OnRawMessageReceived error: " + ex);
            await CloseAsync();
        }
    }

    private async Task HandlePlaidReadyAsync()
    {
        _plaidReady = true;

        if (string.IsNullOrEmpty(_plaidToken))
        {
            Debug.WriteLine("Ready received — waiting for token");
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(100);
                if (!string.IsNullOrEmpty(_plaidToken)) break;
            }
        }

        if (string.IsNullOrEmpty(_plaidToken))
        {
            Debug.WriteLine("Token never arrived — closing");
            await CloseAsync();
            return;
        }

        await InjectTokenAsync();
    }

    private async Task InjectTokenAsync()
    {
        if (_webView == null) return;
        var safeToken = JsonSerializer.Serialize(_plaidToken);
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                _webView.EvaluateJavaScriptAsync($"startPlaid({safeToken})"));

            Debug.WriteLine("Plaid started successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("InjectToken error: " + ex);
            await CloseAsync();
        }
    }

    private async Task HandlePlaidSuccessAsync(JsonElement root)
    {
        var publicToken = root.TryGetProperty("public_token", out var t) ? t.GetString() : null;
        await CloseAsync();
        if (!string.IsNullOrEmpty(publicToken))
            LinkSucceeded?.Invoke(this, publicToken);
    }

    private async Task HandlePlaidExitAsync(JsonElement root)
    {
        if (root.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null)
            Debug.WriteLine("Plaid exit with error: " + err.GetString());

        await CloseAsync();
        LinkClosed?.Invoke(this, EventArgs.Empty);
    }
}