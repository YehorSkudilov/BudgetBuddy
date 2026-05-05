namespace PlaidLink;

public static partial class Plaid
{
    public static void InitializePlatform(string linkToken)
    {
        // Not supported on Windows
    }

    public static void OpenPlatform()
    {
        throw new PlatformNotSupportedException("Plaid is only available on Android");
    }

    public static void RegisterLauncherPlatform(object launcher)
    {
        // no-op
    }
}