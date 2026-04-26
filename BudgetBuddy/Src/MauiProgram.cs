using AppSkeleton;
using Microsoft.Extensions.Logging;
namespace BudgetBuddy
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseAppSkeleton()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddCIcons();
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                System.Diagnostics.Debug.WriteLine($"[CRASH] {ex?.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"[CRASH] Message: {ex?.Message}");
                System.Diagnostics.Debug.WriteLine($"[CRASH] Inner: {ex?.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"[CRASH] Stack: {ex?.StackTrace}");
            };

            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[ANDROID CRASH] {args.Exception?.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"[ANDROID CRASH] Message: {args.Exception?.Message}");
                System.Diagnostics.Debug.WriteLine($"[ANDROID CRASH] Inner: {args.Exception?.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"[ANDROID CRASH] Stack: {args.Exception?.StackTrace}");
                args.Handled = true; // keeps app alive long enough to log
            };
#endif

            return builder.Build();
        }
    }
}
