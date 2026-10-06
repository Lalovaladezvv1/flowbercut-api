namespace Flowbercut.Api.Models.Auth;

public sealed class UsuarioLoginResult
{
    public long Usu_Id { get; init; }

    public long Ten_Id { get; init; }

    public string Usu_NombreUsuario { get; init; } = string.Empty;

    public string Usu_Email { get; init; } = string.Empty;

    public string Usu_ContrasenaHash { get; init; } = string.Empty;

    public bool Usu_EsAdministradorPlataforma { get; init; }

    public string Ten_Codigo { get; init; } = string.Empty;

    public string Ten_Nombre { get; init; } = string.Empty;

    public string Ten_Subdominio { get; init; } = string.Empty;

    public string Ten_ZonaHoraria { get; init; } = string.Empty;

    public int? Rol_Id { get; init; }

    public string? Rol_Codigo { get; init; }

    public string? Rol_Nombre { get; init; }
}