using Flowbercut.Api.Models.Results;

namespace Flowbercut.Api.Repositories;

public interface IPlatformTenantRepository
{
    Task<IReadOnlyCollection<TenantResult>> ObtenerBarberiasAsync(
        CancellationToken cancellationToken);

    Task<TenantResult?> CrearBarberiaAsync(
        string codigo,
        string nombre,
        string subdominio,
        string zonaHoraria,
        long usuarioCreacion,
        CancellationToken cancellationToken);

    Task<TenantAdminResult?> CrearAdministradorAsync(
        long tenantId,
        string nombreUsuario,
        string email,
        string passwordHash,
        long usuarioCreacion,
        CancellationToken cancellationToken);

    Task<TenantAdminResult?> RestablecerContrasenaAdministradorAsync(
        long tenantId,
        string passwordHash,
        long usuarioActualizacion,
        CancellationToken cancellationToken);

    Task<TenantResult?> CambiarEstatusAsync(
        long tenantId,
        int estatusId,
        long usuarioActualizacion,
        CancellationToken cancellationToken);
}