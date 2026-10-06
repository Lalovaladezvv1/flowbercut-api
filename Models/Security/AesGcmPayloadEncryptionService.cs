using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Flowbercut.Api.Models.Security;
using Flowbercut.Api.Options;
using Microsoft.Extensions.Options;

namespace Flowbercut.Api.Security;

public sealed class AesGcmPayloadEncryptionService
    : IPayloadEncryptionService
{
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly PayloadEncryptionOptions _options;

    public AesGcmPayloadEncryptionService(
        IOptions<PayloadEncryptionOptions> options)
    {
        _options = options.Value;
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public PayloadEnvelope Encrypt(
        string plaintext,
        SecuritySession session,
        string requestId,
        string method,
        string path,
        int statusCode)
    {
        if (session.Key.Length != KeySize)
        {
            throw new CryptographicException(
                "La clave de sesión debe tener 256 bits.");
        }

        var iv = RandomNumberGenerator.GetBytes(NonceSize);

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        var ciphertext = new byte[plaintextBytes.Length];

        var tag = new byte[TagSize];

        var aad = BuildResponseAad(
            session.SessionId,
            requestId,
            method,
            path,
            timestamp,
            statusCode);

        using var aes = new AesGcm(session.Key, TagSize);

        aes.Encrypt(
            iv,
            plaintextBytes,
            ciphertext,
            tag,
            aad);

        return new PayloadEnvelope
        {
            Version = 1,
            Algorithm = "A256GCM",
            KeyId = _options.KeyId,
            SessionId = session.SessionId,
            RequestId = requestId,
            Timestamp = timestamp,
            Iv = Convert.ToBase64String(iv),
            Ciphertext = Convert.ToBase64String(ciphertext),
            Tag = Convert.ToBase64String(tag)
        };
    }

    public string Decrypt(
        PayloadEnvelope envelope,
        SecuritySession session,
        string method,
        string path)
    {
        if (session.Key.Length != KeySize)
        {
            throw new CryptographicException(
                "La clave de sesión debe tener 256 bits.");
        }

        if (envelope.Version != 1)
        {
            throw new CryptographicException(
                "Versión de payload no soportada.");
        }

        if (!string.Equals(
                envelope.Algorithm,
                "A256GCM",
                StringComparison.Ordinal))
        {
            throw new CryptographicException(
                "Algoritmo de cifrado no soportado.");
        }

        var iv = Convert.FromBase64String(envelope.Iv);

        var ciphertext =
            Convert.FromBase64String(envelope.Ciphertext);

        var tag =
            Convert.FromBase64String(envelope.Tag);

        if (iv.Length != NonceSize)
        {
            throw new CryptographicException(
                "El IV debe tener 96 bits.");
        }

        if (tag.Length != TagSize)
        {
            throw new CryptographicException(
                "El tag GCM debe tener 128 bits.");
        }

        ValidateTimestamp(envelope.Timestamp);

        var aad = BuildRequestAad(
            session.SessionId,
            envelope.RequestId,
            method,
            path,
            envelope.Timestamp);

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(session.Key, TagSize);

        aes.Decrypt(
            iv,
            ciphertext,
            tag,
            plaintext,
            aad);

        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] BuildRequestAad(
        string sessionId,
        string requestId,
        string method,
        string path,
        long timestamp)
    {
        var value =
            $"request|{sessionId}|{requestId}|{method}|{path}|{timestamp}";

        return Encoding.UTF8.GetBytes(value);
    }

    private static byte[] BuildResponseAad(
        string sessionId,
        string requestId,
        string method,
        string path,
        long timestamp,
        int statusCode)
    {
        var value =
            $"response|{sessionId}|{requestId}|{method}|{path}|{statusCode}|{timestamp}";

        return Encoding.UTF8.GetBytes(value);
    }

    private static void ValidateTimestamp(long timestamp)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var difference = Math.Abs(now - timestamp);

        const long maxDifference =
            30_000;

        if (difference > maxDifference)
        {
            throw new CryptographicException(
                "El payload ha expirado.");
        }
    }
}