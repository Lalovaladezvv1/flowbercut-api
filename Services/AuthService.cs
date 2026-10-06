using Flowbercut.Api.Models.Requests;
using Flowbercut.Api.Models.Responses;
using Flowbercut.Api.Repositories;
using Flowbercut.Api.Security;
using Flowbercut.Api.Services.Interfaces;

namespace Flowbercut.Api.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IAuthRepository authRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _authRepository = authRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var usuarios =
            await _authRepository.ObtenerUsuarioLoginAsync(
                email,
                cancellationToken);

        if (usuarios.Count == 0)
        {
            return null;
        }

        var usuario = usuarios.First();

        var passwordValido =
            _passwordService.Verify(
                request.Password,
                usuario.Usu_ContrasenaHash);

        if (!passwordValido)
        {
            return null;
        }

        var roles = usuarios
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Rol_Codigo))
            .Select(x => x.Rol_Codigo!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var (token, expiresAt) =
            _jwtService.CreateToken(
                usuario,
                roles);

        return new LoginResponse
        {
            AccessToken = token,

            ExpiresAt = expiresAt,

            Usuario = new UsuarioSessionResponse
            {
                Id = usuario.Usu_Id,

                NombreUsuario =
                    usuario.Usu_NombreUsuario,

                Email =
                    usuario.Usu_Email,

                EsAdministradorPlataforma =
                    usuario.Usu_EsAdministradorPlataforma,

                RolCodigo =
                    usuario.Rol_Codigo,

                RolNombre =
                    usuario.Rol_Nombre,

                Tenant = new TenantSessionResponse
                {
                    Id = usuario.Ten_Id,

                    Codigo =
                        usuario.Ten_Codigo,

                    Nombre =
                        usuario.Ten_Nombre,

                    Subdominio =
                        usuario.Ten_Subdominio
                }
            }
        };
    }
}