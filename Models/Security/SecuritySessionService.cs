using System.Collections.Concurrent;

namespace Flowbercut.Api.Security;

public sealed class SecuritySessionService : ISecuritySessionService
{
    private readonly ConcurrentDictionary<string, SecuritySession> _sessions = new();

    private readonly TimeSpan _sessionLifetime;

    private readonly TimeSpan _requestIdLifetime;

    public SecuritySessionService(
        TimeSpan sessionLifetime,
        TimeSpan requestIdLifetime)
    {
        _sessionLifetime = sessionLifetime;
        _requestIdLifetime = requestIdLifetime;
    }

    public SecuritySession CreateSession(byte[] key)
    {
        var now = DateTimeOffset.UtcNow;

        var session = new SecuritySession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            Key = key,
            CreatedAt = now,
            ExpiresAt = now.Add(_sessionLifetime)
        };

        _sessions[session.SessionId] = session;

        return session;
    }

    public bool TryGetSession(
        string sessionId,
        out SecuritySession? session)
    {
        session = null;

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return false;
        }

        if (!_sessions.TryGetValue(
                sessionId,
                out var storedSession))
        {
            return false;
        }

        if (storedSession.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _sessions.TryRemove(
                sessionId,
                out _);

            return false;
        }

        CleanupRequestIds(storedSession);

        session = storedSession;

        return true;
    }

    public bool TryRegisterRequest(
        string sessionId,
        string requestId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) ||
            string.IsNullOrWhiteSpace(requestId))
        {
            return false;
        }

        if (!TryGetSession(
                sessionId,
                out var session) ||
            session is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;

        CleanupRequestIds(session, now);

        return session.UsedRequestIds.TryAdd(
            requestId,
            now);
    }

    public void RemoveSession(string sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            _sessions.TryRemove(
                sessionId,
                out _);
        }
    }

    private void CleanupRequestIds(
        SecuritySession session)
    {
        CleanupRequestIds(
            session,
            DateTimeOffset.UtcNow);
    }

    private void CleanupRequestIds(
        SecuritySession session,
        DateTimeOffset now)
    {
        var expiration =
            now.Subtract(_requestIdLifetime);

        foreach (var request in session.UsedRequestIds)
        {
            if (request.Value <= expiration)
            {
                session.UsedRequestIds.TryRemove(
                    request.Key,
                    out _);
            }
        }
    }
}