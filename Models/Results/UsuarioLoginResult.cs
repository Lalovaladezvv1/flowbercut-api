namespace Flowbercut.Api.Models.Results;

public sealed class UsuarioLoginResult
{
    public long Usu_Id { get; set; }

    public long Ten_Id { get; set; }

    public string Usu_NombreUsuario { get; set; } = string.Empty;

    public string Usu_Email { get; set; } = string.Empty;

    public string Usu_ContrasenaHash { get; set; } = string.Empty;

    public bool Usu_EsAdministradorPlataforma { get; set; }

    public string Ten_Codigo { get; set; } = string.Empty;

    public string Ten_Nombre { get; set; } = string.Empty;

    public string Ten_Subdominio { get; set; } = string.Empty;

    public string? Rol_Codigo { get; set; }

    public string? Rol_Nombre { get; set; }
}
