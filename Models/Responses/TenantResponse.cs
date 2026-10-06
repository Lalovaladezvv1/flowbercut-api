namespace Flowbercut.Api.Models.Responses;

public sealed class TenantResponse
{
    public long Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Subdominio { get; set; } = string.Empty;

    public string ZonaHoraria { get; set; } = string.Empty;

    public int EstatusId { get; set; }

    public string EstatusCodigo { get; set; } = string.Empty;

    public string EstatusNombre { get; set; } = string.Empty;

    public DateTimeOffset FechaCreacion { get; set; }

    public bool TieneUsuario { get; set; }

    public long? UsuarioId { get; set; }

    public string? UsuarioEmail { get; set; }
}