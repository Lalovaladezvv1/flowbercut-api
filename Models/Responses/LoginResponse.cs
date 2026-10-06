namespace Flowbercut.Api.Models.Responses;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public UsuarioSessionResponse Usuario { get; set; } = new();
}

public sealed class UsuarioSessionResponse
{
    public long Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool EsAdministradorPlataforma { get; set; }

    public string? RolCodigo { get; set; }

    public string? RolNombre { get; set; }

    public TenantSessionResponse Tenant { get; set; } = new();
}
