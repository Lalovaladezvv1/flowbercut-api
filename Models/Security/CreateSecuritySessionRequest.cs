namespace Flowbercut.Api.Models.Security;

public sealed class CreateSecuritySessionRequest
{
    public string KeyId { get; set; } = string.Empty;

    public string WrappedKey { get; set; } = string.Empty;
}