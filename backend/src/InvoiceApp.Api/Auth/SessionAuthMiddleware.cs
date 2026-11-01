namespace InvoiceApp.Api.Auth;

/// <summary>Requires a valid bearer token on every /api call except sign-in.</summary>
public class SessionAuthMiddleware
{
    private const string BearerPrefix = "Bearer ";
    private readonly RequestDelegate _next;
    private readonly SessionStore _sessions;

    public SessionAuthMiddleware(RequestDelegate next, SessionStore sessions)
    {
        _next = next;
        _sessions = sessions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var needsSession = path.StartsWithSegments("/api") && !path.StartsWithSegments("/api/auth/login");

        if (needsSession)
        {
            var header = context.Request.Headers.Authorization.ToString();
            var token = header.StartsWith(BearerPrefix) ? header[BearerPrefix.Length..] : null;
            var session = _sessions.Get(token);

            if (session is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            context.Items[HttpContextExtensions.SessionKey] = session;
        }

        await _next(context);
    }
}

public static class HttpContextExtensions
{
    public const string SessionKey = "session";

    public static UserSession GetSession(this HttpContext context) =>
        (UserSession)context.Items[SessionKey]!;
}
