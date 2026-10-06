using System.Collections.Concurrent;

namespace Flowbercut.Api.Security;

public sealed class SecuritySession
{
    public required string SessionId { get; init; }

    public required byte[] Key { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public ConcurrentDictionary<string, DateTimeOffset> UsedRequestIds { get; } = new();
}