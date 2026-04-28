using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BudgetBuddy;

public partial class BanksPage : CContentView, INotifyPropertyChanged
{
    private string _plaidToken;
    private bool _plaidReady = false;

    public BanksPage()
    {
        InitializeComponent();


        BindingContext = this;

        XWebView.Navigating += XWebView_Navigating;

        _ = InitAsync();
    }

    public override void OnAppearing()
    {

        IsWebViewOpen = false;
         LoadBanksAsync();

    }

    // -----------------------------
    // INIT
    // -----------------------------
    private async Task InitAsync()
    {
        try
        {
            await LoadBanksAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Init error: " + ex);
        }
    }

    // -----------------------------
    // BANKS
    // -----------------------------
    private ObservableCollection<BankConnection> _banks = new();

    public ObservableCollection<BankConnection> Banks
    {
        get => _banks;
        set
        {
            if (_banks == value) return;
            _banks = value;
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

    // -----------------------------
    // WEBVIEW STATE
    // -----------------------------
    private bool _isWebViewOpen;

    public bool IsWebViewOpen
    {
        get => _isWebViewOpen;
        set
        {
            if (_isWebViewOpen == value) return;
            _isWebViewOpen = value;
            OnPropertyChanged();
            OnIsWebViewOpenChanged(value);
        }
    }

    // -----------------------------
    // PLAID START
    // -----------------------------
    private async void Add_Clicked(object sender, EventArgs e)
    {
        try
        {

            _plaidToken = await ApiCommunicators.Plaid.CreateLinkTokenAsync();
            _plaidReady = true;

            XWebView.Source = ApiCommunicators.BaseUrl + "/api/plaid.html" ;
                        IsWebViewOpen = true;

        }
        catch (Exception ex)
        {
            Debug.WriteLine("Add_Clicked error: " + ex);
            IsWebViewOpen = false;
        }
    }


    // -----------------------------
    // CALLBACK HANDLER
    // -----------------------------
    private async void XWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Url))
            return;

        // 🔥 NEW: JS is ready → start Plaid here
        if (e.Url.StartsWith("maui://plaid-ready"))
        {
            e.Cancel = true;

            if (_plaidReady && !string.IsNullOrEmpty(_plaidToken))
            {
                var safeToken = _plaidToken.Replace("'", "\\'");

                await XWebView.EvaluateJavaScriptAsync(
                    $"startPlaid('{safeToken}')"
                );
            }

            return;
        }

        // existing success/exit logic
        if (!e.Url.StartsWith("maui://plaid-success") &&
            !e.Url.StartsWith("maui://plaid-exit"))
            return;

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
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Plaid callback error: " + ex);
        }

        IsWebViewOpen = false;
    }

    // -----------------------------
    // RESET WEBVIEW
    // -----------------------------
    private void OnIsWebViewOpenChanged(bool isOpen)
    {
        if (!isOpen)
        {
            _plaidReady = false;
            _plaidToken = null;
            XWebView.Source = "about:blank";
        }
    }

    // -----------------------------
    // PROPERTY CHANGED
    // -----------------------------
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}