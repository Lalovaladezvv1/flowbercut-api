namespace Flowbercut.Api.Models.Security;

public sealed class CreateSecuritySessionResponse
{
    public string SessionId { get; set; } = string.Empty;

    public string KeyId { get; set; } = string.Empty;

    public string Algorithm { get; set; } = "A256GCM";

    public DateTimeOffset ExpiresAt { get; set; }
}