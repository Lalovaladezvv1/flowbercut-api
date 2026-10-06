namespace Flowbercut.Api.Models.Responses;

public sealed class TenantAdminResponse
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}