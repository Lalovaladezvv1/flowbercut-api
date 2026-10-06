namespace Flowbercut.Api.Services.Interfaces;

public interface IPasswordService
{
    string Hash(string password);

    bool Verify(
        string password,
        string passwordHash);
}
