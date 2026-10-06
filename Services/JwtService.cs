using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Flowbercut.Api.Models.Results;
using Flowbercut.Api.Options;
using Flowbercut.Api.Services.Interfaces;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Flowbercut.Api.Services;

public sealed class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }


    public (string Token, DateTimeOffset ExpiresAt) CreateToken(
    UsuarioLoginResult usuario,
    IReadOnlyCollection<string> roles)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException(
                "No se configuró Jwt:SecretKey.");
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Usu_Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Usu_Email),
            new("tenant_id", usuario.Ten_Id.ToString()),
            new("tenant_code", usuario.Ten_Codigo),
            new("tenant_name", usuario.Ten_Nombre),
            new("tenant_subdomain", usuario.Ten_Subdominio),
            new(
                "platform_admin",
                usuario.Usu_EsAdministradorPlataforma
                    .ToString()
                    .ToLowerInvariant())
        };

        foreach (var role in roles.Distinct(
             StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(
                        new Claim(
                            ClaimTypes.Role,
                            role));
                }
            }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
