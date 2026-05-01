using System.Diagnostics;

namespace BudgetBuddy;

public partial class SettingsPage : CContentView
{
    public event Action? LoggedOut;

    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        AuthStore.Logout();

        Debug.WriteLine("User logged out");

        // Notify app to return to AuthView
        LoggedOut?.Invoke();
    }

    private void CButton_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Clicked");

        ApiCommunicators.Plaid.SyncAllBanksAsync();
    }
}