namespace Flowbercut.Api.Options;

public sealed class PayloadEncryptionOptions
{
    public const string SectionName = "PayloadEncryption";

    public string KeyId { get; set; } = "flowbercut-api-01";

    public string PrivateKeyPath { get; set; } = string.Empty;

    public int SessionLifetimeMinutes { get; set; } = 30;

    public int AllowedClockSkewSeconds { get; set; } = 30;
}
