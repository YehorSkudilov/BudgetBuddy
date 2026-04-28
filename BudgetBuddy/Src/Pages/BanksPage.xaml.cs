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
            ShowWebView(false);
        }
        LoadBanksAsync();
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
        {
            return false;
        }

        e.Cancel = true;

        ShowWebView(false);
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
            await ShowWebView(false); // close even on failure
        }
        return false;
    }

    async Task<bool> InitPlaid(WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("maui://plaid-ready"))
        {
            e.Cancel = true;

            if (!string.IsNullOrEmpty(plaidToken))
            {
                var safeToken = plaidToken.Replace("'", "\\'");

                try
                {
                    await XWebView.EvaluateJavaScriptAsync(
                        $"startPlaid('{safeToken}')"
                    );
                    Debug.WriteLine("PlaidInit");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

        }
        return false;
    }

    // -----------------------------
    // CALLBACK HANDLER
    // -----------------------------
    private async void XWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Url)) return;

        if (await InitPlaid(e)) return;
        if (await ExitPlaid(e)) return;
        if (await SuccessPlaid(e)) return;
    }

    // -----------------------------
    // RESET WEBVIEW
    // -----------------------------
    private async Task ShowWebView(bool open)  // Task, not void
    {
        if (open)
        {
            // Fetch token off main thread
            plaidToken = await Task.Run(() =>
                ApiCommunicators.Plaid.CreateLinkTokenAsync());

            if (string.IsNullOrEmpty(plaidToken)) return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                WebViewBorder.IsVisible = true;
                XWebView.Source = ApiCommunicators.BaseUrl + "/api/plaid.html";
            });
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                XWebView.Source = "about:blank");

            await Task.Delay(500); // reduced from 1000ms
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