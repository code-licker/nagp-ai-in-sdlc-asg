using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace InvoiceApp.Api.Auth;

/// <summary>Keeps signed-in sessions in memory, keyed by an opaque bearer token.</summary>
public class SessionStore
{
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();

    public string Create(UserSession session)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        _sessions[token] = session;
        return token;
    }

    public UserSession? Get(string? token) =>
        token is not null && _sessions.TryGetValue(token, out var session) ? session : null;

    public void Remove(string token) => _sessions.TryRemove(token, out _);
}
