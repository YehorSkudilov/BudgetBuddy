using System.Text;
using Microsoft.AspNetCore.Http;

namespace BudgetBuddyAPI;

public class SwaggerAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SwaggerAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        const string realm = "Swagger";

        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            Challenge(context, realm);
            return;
        }

        var auth = authHeader.ToString();
        if (!auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(context, realm);
            return;
        }

        string decoded;
        try
        {
            var encoded = auth["Basic ".Length..].Trim();
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            Challenge(context, realm);
            return;
        }

        var parts = decoded.Split(':', 2);
        if (parts.Length != 2)
        {
            Challenge(context, realm);
            return;
        }

        var username = parts[0];
        var password = parts[1];

        // same behavior as second class (no DI/env distinction required)
        var expectedUser = SwaggerCredentials.User;
        var expectedPass = SwaggerCredentials.Pass;

        if (string.IsNullOrEmpty(expectedUser) || string.IsNullOrEmpty(expectedPass))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Swagger credentials not configured.");
            return;
        }

        if (username != expectedUser || password != expectedPass)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }

    private static void Challenge(HttpContext context, string realm)
    {
        context.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{realm}\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
}

// simple shared config holder to match behavior (not source-specific)
public static class SwaggerCredentials
{
    public static string? User { get; set; }
    public static string? Pass { get; set; }
}