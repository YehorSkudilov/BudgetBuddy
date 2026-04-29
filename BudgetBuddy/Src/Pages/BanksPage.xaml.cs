using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BudgetBuddy;

public partial class BanksPage : CContentView, INotifyPropertyChanged
{
    private string plaidToken;
    private bool webView_Loaded;

    public BanksPage()
    {
        InitializeComponent();

        // Pre-warm assembly to avoid JIT lag on first Plaid callback
        _ = System.Web.HttpUtility.ParseQueryString("");

        XWebView.Navigating += XWebView_Navigating;
        XWebView.Loaded += XWebView_Loaded;
        LoadBanksAsync();
        BindingContext = this;
    }

    // -----------------------------
    // BANKS
    // -----------------------------
    private ObservableCollection<BankConnection> banks = new();

    public ObservableCollection<BankConnection> Banks
    {
        get => banks;
        set
        {
            if (banks == value) return;
            banks = value;
            OnPropertyChanged();
        }
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
        catch (Exception ex)
        {
            Debug.WriteLine("LoadBanks error: " + ex);
        }
    }

    public async override void OnAppearing()
    {
        if (webView_Loaded)
        {
            await ShowWebView(false);
        }
        await LoadBanksAsync();
    }

    // -----------------------------
    // PLAID START
    // -----------------------------
    private async void Add_Clicked(object sender, EventArgs e)
    {
        await ShowWebView(true);
    }

    async Task<bool> ExitPlaid(WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("maui://plaid-exit"))
            return false;

        e.Cancel = true;

        await ShowWebView(false);
        Debug.WriteLine("ExitedPlaid");
        return true;
    }

    async Task<bool> SuccessPlaid(WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("maui://plaid-success"))
            return false;

        e.Cancel = true;

        try
        {
            var uri = new Uri(e.Url);
            var data = System.Web.HttpUtility
                .ParseQueryString(uri.Query).Get("data");

            if (string.IsNullOrEmpty(data)) return false;

            var json = Uri.UnescapeDataString(data);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("public_token", out var token))
            {
                var publicToken = token.GetString();

                // Close UI immediately, then do network work
                await ShowWebView(false);
                await ApiCommunicators.Plaid.ExchangePublicTokenAsync(publicToken);
                await LoadBanksAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SuccessPlaid error: " + ex);
            await ShowWebView(false);
        }
        return false;
    }

    async Task<bool> InitPlaid(WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("maui://plaid-ready"))
            return false;

        e.Cancel = true;
        await InjectPlaidToken();
        return true;
    }

    // -----------------------------
    // PLAID INJECTION (with retry)
    // -----------------------------
    private async Task InjectPlaidToken()
    {
        if (string.IsNullOrEmpty(plaidToken)) return;

        var safeToken = plaidToken.Replace("'", "\\'");

        for (int i = 0; i < 5; i++)
        {
            try
            {
                var result = await MainThread.InvokeOnMainThreadAsync(() =>
                    XWebView.EvaluateJavaScriptAsync(
                        $"typeof startPlaid === 'function' ? startPlaid('{safeToken}') : 'not_ready'"
                    ));

                if (result != "not_ready")
                {
                    Debug.WriteLine("PlaidInit success");
                    return;
                }

                Debug.WriteLine($"PlaidInit retry {i + 1}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PlaidInit attempt {i + 1} failed: {ex.Message}");
            }

            await Task.Delay(300);
        }

        Debug.WriteLine("PlaidInit failed after all retries");
    }

    // -----------------------------
    // CALLBACK HANDLER
    // -----------------------------
    private async void XWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine($"Navigating: {e.Url}");

        if (string.IsNullOrEmpty(e.Url)) return;

        if (await InitPlaid(e)) return;
        if (await ExitPlaid(e)) return;
        if (await SuccessPlaid(e)) return;
    }

    // -----------------------------
    // SHOW / HIDE WEBVIEW
    // -----------------------------
    private async Task ShowWebView(bool open)
    {
        if (open)
        {
            // Fetch token before loading page so it's ready when plaid-ready fires
            plaidToken = await Task.Run(() =>
                ApiCommunicators.Plaid.CreateLinkTokenAsync());

            if (string.IsNullOrEmpty(plaidToken))
            {
                Debug.WriteLine("Failed to get Plaid token");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                WebViewBorder.IsVisible = true;
                XWebView.Source = ApiCommunicators.BaseUrl + "/api/plaid.html";
            });

            // Fallback: if plaid-ready signal was missed on first launch, poll and inject
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000); // give the page time to fully load

                if (string.IsNullOrEmpty(plaidToken)) return; // already handled via signal

                try
                {
                    var isReady = await MainThread.InvokeOnMainThreadAsync(() =>
                        XWebView.EvaluateJavaScriptAsync("window._plaidReady === true ? 'yes' : 'no'"));

                    if (isReady == "yes")
                    {
                        Debug.WriteLine("Fallback injection triggered");
                        await InjectPlaidToken();
                    }
                    else
                    {
                        Debug.WriteLine("Fallback: page not ready after 2s");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fallback injection error: " + ex.Message);
                }
            });
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                XWebView.Source = "about:blank");

            await Task.Delay(500);
            plaidToken = null;

            await MainThread.InvokeOnMainThreadAsync(() =>
                WebViewBorder.IsVisible = false);
        }
    }

    private void XWebView_Loaded(object sender, EventArgs e)
    {
        webView_Loaded = true;
    }
}