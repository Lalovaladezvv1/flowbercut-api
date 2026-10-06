using Flowbercut.Api.Models.Requests;
using Flowbercut.Api.Models.Responses;
using Flowbercut.Api.Repositories;
using Flowbercut.Api.Services.Interfaces;

namespace Flowbercut.Api.Services;

public sealed class PlatformTenantService
    : IPlatformTenantService
{
    private readonly IPlatformTenantRepository
        _tenantRepository;

    private readonly IPasswordService
        _passwordService;

    public PlatformTenantService(
        IPlatformTenantRepository tenantRepository,
        IPasswordService passwordService)
    {
        _tenantRepository =
            tenantRepository;

        _passwordService =
            passwordService;
    }

    public async Task<IReadOnlyCollection<TenantResponse>>
        ObtenerBarberiasAsync(
            CancellationToken cancellationToken)
    {
        var barberias =
            await _tenantRepository
                .ObtenerBarberiasAsync(
                    cancellationToken);

        return barberias
            .Select(x => new TenantResponse
            {
                Id = x.Ten_Id,
                Codigo = x.Ten_Codigo,
                Nombre = x.Ten_Nombre,
                Subdominio = x.Ten_Subdominio,
                ZonaHoraria = x.Ten_ZonaHoraria,
                EstatusId = x.Est_Id,
                EstatusCodigo = x.Est_Codigo,
                EstatusNombre = x.Est_Nombre,
                FechaCreacion =
                    x.Aud_FechaCreacion,

                TieneUsuario =
                    x.Usu_Id.HasValue,

                UsuarioId =
                    x.Usu_Id,

                UsuarioEmail =
                    x.Usu_Email
            })
            .ToArray();
    }

    public async Task<TenantResponse?>
        CrearBarberiaAsync(
            CreateTenantRequest request,
            long usuarioCreacion,
            CancellationToken cancellationToken)
    {
        var codigo =
            request.Codigo.Trim().ToUpperInvariant();

        var nombre =
            request.Nombre.Trim();

        var subdominio =
            request.Subdominio.Trim()
                .ToLowerInvariant();

        var zonaHoraria =
            request.ZonaHoraria.Trim();

        if (string.IsNullOrWhiteSpace(codigo) ||
            string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(subdominio) ||
            string.IsNullOrWhiteSpace(zonaHoraria))
        {
            throw new ArgumentException(
                "Todos los datos de la barbería son obligatorios.");
        }

        var barberia =
            await _tenantRepository.CrearBarberiaAsync(
                codigo,
                nombre,
                subdominio,
                zonaHoraria,
                usuarioCreacion,
                cancellationToken);

        if (barberia is null)
        {
            return null;
        }

        return MapTenantResponse(barberia);
    }

    public async Task<TenantAdminResponse?>
        CrearAdministradorAsync(
            long tenantId,
            CreateTenantAdminRequest request,
            long usuarioCreacion,
            CancellationToken cancellationToken)
    {
        var email =
            request.Email.Trim()
                .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "El correo electrónico es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "La contraseña es obligatoria.");
        }

        var passwordHash =
            _passwordService.Hash(
                request.Password);

        var administrador =
            await _tenantRepository
                .CrearAdministradorAsync(
                    tenantId,
                    email,
                    email,
                    passwordHash,
                    usuarioCreacion,
                    cancellationToken);

        if (administrador is null)
        {
            return null;
        }

        return new TenantAdminResponse
        {
            Id = administrador.Usu_Id,
            TenantId = administrador.Ten_Id,
            NombreUsuario =
                administrador.Usu_NombreUsuario,
            Email =
                administrador.Usu_Email
        };
    }

    public async Task<TenantAdminResponse?>
        RestablecerContrasenaAdministradorAsync(
            long tenantId,
            ResetTenantAdminPasswordRequest request,
            long usuarioActualizacion,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "La contraseña es obligatoria.");
        }

        var passwordHash =
            _passwordService.Hash(
                request.Password);

        var administrador =
            await _tenantRepository
                .RestablecerContrasenaAdministradorAsync(
                    tenantId,
                    passwordHash,
                    usuarioActualizacion,
                    cancellationToken);

        if (administrador is null)
        {
            return null;
        }

        return new TenantAdminResponse
        {
            Id = administrador.Usu_Id,
            TenantId = administrador.Ten_Id,
            NombreUsuario =
                administrador.Usu_NombreUsuario,
            Email =
                administrador.Usu_Email
        };
    }

    public async Task<TenantResponse?>
        CambiarEstatusAsync(
            long tenantId,
            int estatusId,
            long usuarioActualizacion,
            CancellationToken cancellationToken)
    {
        if (estatusId is not 1 and not 2)
        {
            throw new ArgumentException(
                "El estatus de la barbería no es válido.");
        }

        var barberia =
            await _tenantRepository
                .CambiarEstatusAsync(
                    tenantId,
                    estatusId,
                    usuarioActualizacion,
                    cancellationToken);

        if (barberia is null)
        {
            return null;
        }

        return MapTenantResponse(barberia);
    }

    private static TenantResponse
        MapTenantResponse(
            Models.Results.TenantResult tenant)
    {
        return new TenantResponse
        {
            Id = tenant.Ten_Id,
            Codigo = tenant.Ten_Codigo,
            Nombre = tenant.Ten_Nombre,
            Subdominio = tenant.Ten_Subdominio,
            ZonaHoraria = tenant.Ten_ZonaHoraria,
            EstatusId = tenant.Est_Id,
            EstatusCodigo = tenant.Est_Codigo,
            EstatusNombre = tenant.Est_Nombre,
            FechaCreacion =
                tenant.Aud_FechaCreacion,

            TieneUsuario =
                tenant.Usu_Id.HasValue,

            UsuarioId =
                tenant.Usu_Id,

            UsuarioEmail =
                tenant.Usu_Email
        };
    }
}