using Flowbercut.Api.Models.Results;

namespace Flowbercut.Api.Services.Interfaces;

public interface IJwtService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(
        UsuarioLoginResult usuario,
        IReadOnlyCollection<string> roles);
}