namespace Flowbercut.Api.Models.Requests;

public sealed class ResetTenantAdminPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}