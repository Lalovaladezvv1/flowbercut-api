using Flowbercut.Api.Models.Results;

namespace Flowbercut.Api.Repositories;

public interface IAuthRepository
{
    Task<IReadOnlyCollection<UsuarioLoginResult>> ObtenerUsuarioLoginAsync(
        string email,
        CancellationToken cancellationToken);
}