namespace Flowbercut.Api.Security;

public interface ISecuritySessionService
{
    SecuritySession CreateSession(byte[] key);

    bool TryGetSession(
        string sessionId,
        out SecuritySession? session);

    bool TryRegisterRequest(
        string sessionId,
        string requestId);

    void RemoveSession(string sessionId);
}