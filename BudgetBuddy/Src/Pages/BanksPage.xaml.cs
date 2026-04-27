using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BudgetBuddy;

public partial class BanksPage : ContentView, INotifyPropertyChanged
{
    private readonly Finance _finance;

    public BanksPage()
    {
        InitializeComponent();

        _finance = new Finance();


        XWebView.Navigating += XWebView_Navigating;

       LoadBanksAsync();
       MainGrid.BindingContext = this;

    }

    // -----------------------------
    // REACTIVE BANKS COLLECTION
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

    // -----------------------------
    // LOAD BANKS FROM API
    // -----------------------------
    private async Task LoadBanksAsync()
    {
        try
        {
            var result = await _finance.GetAllBanksAsync();

            if (result == null)
                return;

            Banks = new ObservableCollection<BankConnection>(result);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("LoadBanks error: " + ex.Message);
        }
    }

    // -----------------------------
    // PLAID STATE
    // -----------------------------
    private bool isWebViewOpen;

    public bool IsWebViewOpen
    {
        get => isWebViewOpen;
        set
        {
            if (isWebViewOpen == value) return;

            isWebViewOpen = value;
            OnPropertyChanged();
            OnIsWebViewOpenChanged(value);
        }
    }

    // -----------------------------
    // START PLAID FLOW
    // -----------------------------
    private async void Add_Clicked(object sender, EventArgs e)
    {
        IsWebViewOpen = true;

        var token = await ApiCommunicators.Plaid.CreateLinkTokenAsync();

        XWebView.Source = $"{ApiCommunicators.BaseUrl}/plaid.html";

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
    // HANDLE PLAID CALLBACKS
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

                        if (doc.RootElement.TryGetProperty("public_token", out var tokenElement))
                        {
                            var publicToken = tokenElement.GetString();

                            Debug.WriteLine("PUBLIC TOKEN: " + publicToken);

                            await ApiCommunicators.Plaid
                                .ExchangePublicTokenAsync(publicToken);

                            // 🔥 refresh banks after linking
                            await LoadBanksAsync();
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
    // WEBVIEW STATE RESET
    // -----------------------------
    private void OnIsWebViewOpenChanged(bool isOpen)
    {
        if (!isOpen)
        {
            XWebView.Source = "about:blank";
        }
    }

    // -----------------------------
    // PROPERTY CHANGED (UI NOTIFY)
    // -----------------------------
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}