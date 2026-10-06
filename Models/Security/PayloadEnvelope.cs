namespace Flowbercut.Api.Models.Security;

public sealed class PayloadEnvelope
{
    public int Version { get; set; }

    public string Algorithm { get; set; } = string.Empty;

    public string KeyId { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;

    public long Timestamp { get; set; }

    public string Iv { get; set; } = string.Empty;

    public string Ciphertext { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;
}