namespace Flowbercut.Api.Models.Results;

public sealed class TenantAdminResult
{
    public long Usu_Id { get; set; }

    public long Ten_Id { get; set; }

    public string Usu_NombreUsuario { get; set; } = string.Empty;

    public string Usu_Email { get; set; } = string.Empty;
}