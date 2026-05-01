using AppSkeleton;
using System.ComponentModel;

namespace BudgetBuddy;

public partial class AuthView : CContentView, INotifyPropertyChanged
{
    public event Action? Authenticated;

    private bool isRegisterMode;

    public string ToggleText =>
        isRegisterMode ? "Switch to Login" : "Switch to Register";

    public AuthView()
    {
        InitializeComponent();
        BindingContext = this;

        UpdateMode();
        StayLoggedInCheckBox.IsChecked = true;
    }

    // ---------------- TOGGLE ----------------
    private void OnToggleMode(object sender, TappedEventArgs e)
    {
        isRegisterMode = !isRegisterMode;
        UpdateMode();
    }

    private void UpdateMode()
    {
        ConfirmPasswordEntry.IsVisible = isRegisterMode;

        LoginButton.IsVisible = !isRegisterMode;
        RegisterButton.IsVisible = isRegisterMode;

        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = string.Empty;

        PasswordEntry.Text = string.Empty;
        ConfirmPasswordEntry.Text = string.Empty;

        OnPropertyChanged(nameof(ToggleText));
    }

    // ---------------- LOGIN ----------------
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ShowError("Email and password are required.");
            return;
        }

        await HandleAuth(() =>
            ApiCommunicators.User.LoginAsync(email, password)
        );
    }

    // ---------------- REGISTER ----------------
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirm = ConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirm))
        {
            ShowError("All fields are required.");
            return;
        }

        if (password != confirm)
        {
            ShowError("Passwords do not match.");
            return;
        }

        await HandleAuth(() =>
            ApiCommunicators.User.RegisterAsync(email, "user", password)
        );
    }

    // ---------------- AUTH HANDLER ----------------
    private async Task HandleAuth(Func<Task<AuthResponse?>> action)
    {
        ErrorLabel.IsVisible = false;

        try
        {
            var res = await action();

            if (res == null || string.IsNullOrEmpty(res.token))
            {
                ShowError("Authentication failed.");
                return;
            }

            await AuthStore.SetTokenAsync(
                res.token,
                StayLoggedInCheckBox.IsChecked
            );

            Authenticated?.Invoke();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    // ---------------- INotifyPropertyChanged ----------------
    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}