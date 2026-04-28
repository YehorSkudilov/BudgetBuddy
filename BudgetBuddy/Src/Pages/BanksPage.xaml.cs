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


        XWebView.Navigating += XWebView_Navigating;

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
        ShowWebView(true);
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
        {
            return false;
        }

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

                if (e.Url.StartsWith("maui://plaid-success"))
                {
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("public_token", out var token))
                    {
                        var publicToken = token.GetString();

                        await ApiCommunicators.Plaid.ExchangePublicTokenAsync(publicToken);

                        await LoadBanksAsync();
                        Debug.WriteLine("SuccessPlaid");

                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
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
        if (string.IsNullOrEmpty(e.Url))
            return;

        bool initPlaid = await InitPlaid(e);

        bool exitPlaid = await ExitPlaid(e);

        bool successPaid = await SuccessPlaid(e);

        //ShowWebView(!exitPlaid || !successPaid);
    }

    // -----------------------------
    // RESET WEBVIEW
    // -----------------------------
    private async void ShowWebView(bool open)
    {
        if (open)
        {
            WebViewBorder.IsVisible = true;
            plaidToken = await ApiCommunicators.Plaid.CreateLinkTokenAsync();
            XWebView.Source = ApiCommunicators.BaseUrl + "/api/plaid.html";
        }
        else
        {
            XWebView.Source = "about:blank";
            await Task.Delay(1000);
            plaidToken = null;
            WebViewBorder.IsVisible = false;

        }
    }

    private void XWebView_Loaded(object sender, EventArgs e)
    {
        webView_Loaded = true;
    }
}