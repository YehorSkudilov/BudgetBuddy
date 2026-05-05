using BudgetBuddy;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace BudgetBuddy;
public partial class BanksPage : CContentView, INotifyPropertyChanged
{
    public BanksPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadBanksAsync();
    }

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
        if (PlaidLinkWebView.IsVisible)
            await PlaidLinkWebView.CloseAsync();

        await LoadBanksAsync();
    }

    private async void Add_Clicked(object sender, EventArgs e)
    {

        await PlaidLinkWebView.OpenAsync();
    }

    private async void PlaidLink_LinkSucceeded(object? sender, string publicToken)
    {
        await ApiCommunicators.Plaid.ExchangePublicTokenAsync(publicToken);
        await LoadBanksAsync();
    }

    private void PlaidLink_LinkClosed(object? sender, EventArgs e) { /* optional: show a toast, etc. */ }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}