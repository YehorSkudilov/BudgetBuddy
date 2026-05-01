using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BudgetBuddy;

public partial class BanksPage : CContentView, INotifyPropertyChanged
{
    private string? _plaidToken;
    private bool _plaidReady;

    public BanksPage()
    {
        InitializeComponent();
        BindingContext = this;

        // wwwroot in Resources/Raw is the default — HybridRoot not needed
        XWebView.DefaultFile = "plaid.html";
        XWebView.RawMessageReceived += OnRawMessageReceived;

        LoadBanksAsync();
    }

    // ---------------------------------------------------------------
    // BANKS
    // ---------------------------------------------------------------
    private ObservableCollection<BankConnection> _banks = new();
    public ObservableCollection<BankConnection> Banks
    {
        get => _banks;
        set { if (_banks == value) return; _banks = value; OnPropertyChanged(); }
    }

    private async Task LoadBanksAsync()
    {
        try
        {
            var result = await ApiCommunicators.Finance.GetAllBanksAsync();
            Banks = result != null
                ? new ObservableCollection<BankConnection>(result)
                : new ObservableCollection<BankConnection>();
        }
        catch (Exception ex) { Debug.WriteLine("LoadBanks error: " + ex); }
    }

    public override async void OnAppearing()
    {
        if (WebViewBorder.IsVisible)
            await CloseWebView();

        await LoadBanksAsync();
    }

    // ---------------------------------------------------------------
    // OPEN / CLOSE
    // ---------------------------------------------------------------
    private async void Add_Clicked(object sender, EventArgs e) => await OpenWebView();

    private async Task OpenWebView()
    {
        var tokenTask = Task.Run(() => ApiCommunicators.Plaid.CreateLinkTokenAsync());

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Navigate back to plaid.html from about:blank BEFORE showing
            XWebView.EvaluateJavaScriptAsync("window.location.href = 'plaid.html'");
            WebViewBorder.IsVisible = true;
        });

        _plaidToken = await tokenTask;
        if (string.IsNullOrEmpty(_plaidToken))
        {
            Debug.WriteLine("Failed to get Plaid token");
            await CloseWebView();
            return;
        }

        if (_plaidReady)
            await InjectToken();
    }

    private async Task CloseWebView()
    {
        _plaidToken = null;
        _plaidReady = false;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            WebViewBorder.IsVisible = false;

            try
            {
                // Navigate to blank immediately — no visible flash on next open
                XWebView.EvaluateJavaScriptAsync("window.location.href = 'about:blank'");
            }
            catch { /* ignore */ }
        });
    }

    // ---------------------------------------------------------------
    // MESSAGE BRIDGE
    // ---------------------------------------------------------------
    private async void OnRawMessageReceived(object? sender, HybridWebViewRawMessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Message)) return;

        Debug.WriteLine($"[HybridWebView] Message: {e.Message}");

        try
        {
            using var doc = JsonDocument.Parse(e.Message);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "ready":
                    await HandlePlaidReady();
                    break;

                case "success":
                    await HandlePlaidSuccess(root);
                    break;

                case "exit":
                    await HandlePlaidExit(root);
                    break;

                default:
                    Debug.WriteLine($"Unknown message type: {type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OnRawMessageReceived error: " + ex);
            await CloseWebView();
        }
    }

    private async Task HandlePlaidReady()
    {
        _plaidReady = true;

        // Token might not be fetched yet if network was slow — wait for it
        if (string.IsNullOrEmpty(_plaidToken))
        {
            Debug.WriteLine("Ready received — waiting for token");

            for (int i = 0; i < 40; i++) // up to 4 seconds
            {
                await Task.Delay(100);
                if (!string.IsNullOrEmpty(_plaidToken)) break;
            }
        }

        if (string.IsNullOrEmpty(_plaidToken))
        {
            Debug.WriteLine("Token never arrived — closing");
            await CloseWebView();
            return;
        }

        await InjectToken();
    }

    private async Task InjectToken()
    {
        // Escape token for safe JS string injection
        var safeToken = JsonSerializer.Serialize(_plaidToken); // gives "token-with-quotes-safe"

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                XWebView.EvaluateJavaScriptAsync($"startPlaid({safeToken})"));

            Debug.WriteLine("Plaid started successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("InjectToken error: " + ex);
            await CloseWebView();
        }
    }

    private async Task HandlePlaidSuccess(JsonElement root)
    {
        var publicToken = root.TryGetProperty("public_token", out var t) ? t.GetString() : null;

        await CloseWebView();

        if (!string.IsNullOrEmpty(publicToken))
        {
            await ApiCommunicators.Plaid.ExchangePublicTokenAsync(publicToken);
            await LoadBanksAsync();
        }
    }

    private async Task HandlePlaidExit(JsonElement root)
    {
        if (root.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null)
            Debug.WriteLine("Plaid exit with error: " + err.GetString());

        await CloseWebView();
    }

    // ---------------------------------------------------------------
    // INotifyPropertyChanged
    // ---------------------------------------------------------------
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}