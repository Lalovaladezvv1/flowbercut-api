using Flowbercut.Api.Models.Requests;
using Flowbercut.Api.Models.Responses;

namespace Flowbercut.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}
