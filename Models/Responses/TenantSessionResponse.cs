namespace Flowbercut.Api.Models.Responses;

public sealed class TenantSessionResponse
{
    public long Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Subdominio { get; set; } = string.Empty;
}
