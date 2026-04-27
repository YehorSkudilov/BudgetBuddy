using System.Diagnostics;

namespace BudgetBuddy;

public partial class SettingsPage : ContentView
{
	public SettingsPage()
	{
		InitializeComponent();
	}

    private void CButton_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Clicked");

        ApiCommunicators.Plaid.SyncAllBanksAsync();
    }
}