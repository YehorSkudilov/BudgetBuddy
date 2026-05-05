namespace PlaidLink;

using Android.App;
using Android.Util;
using AndroidX.Activity.Result;
using Com.Plaid.Link;
using Com.Plaid.Link.Configuration;

public static partial class Plaid
{
    private static PlaidHandler? _handler;
    private static ActivityResultLauncher? _launcher;
    private static Activity? _activity;

    public static void InitializePlatform(string linkToken)
    {
        _activity = Platform.CurrentActivity;

        if (_activity?.Application == null)
            throw new Exception("No Android activity");

        var config = new LinkTokenConfiguration.Builder()
            .InvokeToken(linkToken)
            .Build();

        _handler = Com.Plaid.Link.Plaid.Create(_activity.Application, config);
    }

    public static void OpenPlatform()
    {
        if (_handler == null)
            throw new Exception("Plaid not initialized");

        if (_launcher == null)
            throw new Exception("Launcher not registered in MainActivity");

        _launcher.Launch((Java.Lang.Object)_handler);
    }

    public static void RegisterLauncher(ActivityResultLauncher launcher)
    {
        _launcher = launcher;
    }

    public static void SetEventListener()
    {
        try
        {
            var method = typeof(Plaid).GetMethod("SetLinkEventListener");

            method?.Invoke(null, new object[]
            {
                new Action<object>(e =>
                {
                    Log.Info("Plaid", e?.ToString() ?? "null");
                })
            });
        }
        catch { }
    }
}