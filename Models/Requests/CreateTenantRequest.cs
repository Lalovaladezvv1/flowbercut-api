namespace Flowbercut.Api.Models.Requests;

public sealed class CreateTenantRequest
{
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Subdominio { get; set; } = string.Empty;

    public string ZonaHoraria { get; set; } = string.Empty;
}