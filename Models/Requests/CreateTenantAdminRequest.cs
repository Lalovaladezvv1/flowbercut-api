namespace Flowbercut.Api.Models.Requests;

public sealed class CreateTenantAdminRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}