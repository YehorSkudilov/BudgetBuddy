namespace PlaidLink;

public static partial class Plaid
{
    public static void Initialize(string linkToken)
    {
        InitializePlatform(linkToken);
    }

    public static void Open()
    {
        OpenPlatform();
    }
}