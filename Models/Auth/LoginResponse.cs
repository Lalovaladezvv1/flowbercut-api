namespace Flowbercut.Api.Models.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; init; }

    public UsuarioLoginData Usuario { get; init; } = new();
}

public sealed class UsuarioLoginData
{
    public long UsuarioId { get; init; }

    public string NombreUsuario { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool EsAdministradorPlataforma { get; init; }

    public TenantLoginData Tenant { get; init; } = new();

    public IReadOnlyCollection<string> Roles { get; init; } =
        Array.Empty<string>();
}

public sealed class TenantLoginData
{
    public long Id { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Subdominio { get; init; } = string.Empty;

    public string ZonaHoraria { get; init; } = string.Empty;
}