using Flowbercut.Api.Models.Security;

namespace Flowbercut.Api.Security;

public interface IPayloadEncryptionService
{
    PayloadEnvelope Encrypt(
        string plaintext,
        SecuritySession session,
        string requestId,
        string method,
        string path,
        int statusCode);

    string Decrypt(
        PayloadEnvelope envelope,
        SecuritySession session,
        string method,
        string path);
}