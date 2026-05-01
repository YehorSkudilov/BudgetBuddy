namespace BudgetBuddy;

public static class AuthStore
{
    private const string TokenKey = "auth_token";

    public static string? Token { get; set; }

    // 🔥 App-level logout event (UI listens to this)
    public static event Action? AuthChanged;

    public static async Task InitializeAsync()
    {
#if ANDROID || IOS || MACCATALYST
        Token = await SecureStorage.GetAsync(TokenKey);
#elif WINDOWS
        Token = LoadFromFile();
#else
        Token = null;
#endif
    }

    public static async Task SetTokenAsync(string token, bool persist)
    {
        Token = token;

        if (!persist)
            return;

#if ANDROID || IOS || MACCATALYST
        await SecureStorage.SetAsync(TokenKey, token);
#elif WINDOWS
        SaveToFile(token);
#endif
    }

    // ✅ CLEAN LOGOUT METHOD
    public static void Logout()
    {
        Token = null;

#if ANDROID || IOS || MACCATALYST
        SecureStorage.Remove(TokenKey);
#elif WINDOWS
        DeleteFile();
#endif

        AuthChanged?.Invoke(); // notify UI
    }

#if WINDOWS
    private static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "budgetbuddy_token.txt");

    private static void SaveToFile(string token)
        => File.WriteAllText(FilePath, token);

    private static string? LoadFromFile()
        => File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;

    private static void DeleteFile()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
#endif
}