using Flowbercut.Api.Models.Requests;
using Flowbercut.Api.Models.Responses;

namespace Flowbercut.Api.Services.Interfaces;

public interface IPlatformTenantService
{
    Task<IReadOnlyCollection<TenantResponse>>
        ObtenerBarberiasAsync(
            CancellationToken cancellationToken);

    Task<TenantResponse?> CrearBarberiaAsync(
        CreateTenantRequest request,
        long usuarioCreacion,
        CancellationToken cancellationToken);

    Task<TenantAdminResponse?> CrearAdministradorAsync(
        long tenantId,
        CreateTenantAdminRequest request,
        long usuarioCreacion,
        CancellationToken cancellationToken);

    Task<TenantAdminResponse?> RestablecerContrasenaAdministradorAsync(
        long tenantId,
        ResetTenantAdminPasswordRequest request,
        long usuarioActualizacion,
        CancellationToken cancellationToken);

    Task<TenantResponse?> CambiarEstatusAsync(
        long tenantId,
        int estatusId,
        long usuarioActualizacion,
        CancellationToken cancellationToken);
}