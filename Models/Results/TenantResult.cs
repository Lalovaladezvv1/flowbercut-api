namespace Flowbercut.Api.Models.Results;

public sealed class TenantResult
{
    public long Ten_Id { get; set; }

    public string Ten_Codigo { get; set; } = string.Empty;

    public string Ten_Nombre { get; set; } = string.Empty;

    public string Ten_Subdominio { get; set; } = string.Empty;

    public string Ten_ZonaHoraria { get; set; } = string.Empty;

    public int Est_Id { get; set; }

    public string Est_Codigo { get; set; } = string.Empty;

    public string Est_Nombre { get; set; } = string.Empty;

    public DateTimeOffset Aud_FechaCreacion { get; set; }

    public long? Usu_Id { get; set; }

    public string? Usu_Email { get; set; }
}