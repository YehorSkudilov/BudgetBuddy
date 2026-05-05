using Android.App;
using Android.OS;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Com.Plaid.Link;
using Com.Plaid.Link.Result;

namespace YourAppNamespace;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var launcher =
            RegisterForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                new PlaidCallback());

        PlaidLink.Plaid.RegisterLauncher(launcher);
    }

    // ==========================================
    // SIMPLE INLINE CALLBACK (NO EXTRA CLASS NEEDED)
    // ==========================================
    private class PlaidCallback : Java.Lang.Object, IActivityResultCallback
    {
        public void OnActivityResult(Java.Lang.Object result)
        {
            if (result is LinkSuccess success)
            {
                System.Diagnostics.Debug.WriteLine(success.PublicToken);
            }
            else if (result is LinkExit exit)
            {
                System.Diagnostics.Debug.WriteLine(exit.Error?.ErrorMessage);
            }
        }
    }
}