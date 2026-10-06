using System.Security.Cryptography;
using Flowbercut.Api.Models.Security;
using Flowbercut.Api.Options;
using Flowbercut.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Flowbercut.Api.Controllers;

[ApiController]
[Route("api/v1/security")]
public sealed class SecurityController : ControllerBase
{
    private readonly PayloadEncryptionOptions _options;
    private readonly ISecuritySessionService _sessionService;
    private readonly IWebHostEnvironment _environment;

    public SecurityController(
        IOptions<PayloadEncryptionOptions> options,
        ISecuritySessionService sessionService,
        IWebHostEnvironment environment)
    {
        _options = options.Value;
        _sessionService = sessionService;
        _environment = environment;
    }

    [HttpGet("public-key")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPublicKey()
    {
        using var rsa = LoadPrivateKey();

        var publicKey =
            rsa.ExportSubjectPublicKeyInfoPem();

        return Ok(new
        {
            keyId = _options.KeyId,
            algorithm = "RSA-OAEP-256",
            publicKey
        });
    }

    [HttpPost("session")]
    [ProducesResponseType(
        typeof(CreateSecuritySessionResponse),
        StatusCodes.Status200OK)]
    public IActionResult CreateSession(
        [FromBody] CreateSecuritySessionRequest request)
    {
        if (!string.Equals(
                request.KeyId,
                _options.KeyId,
                StringComparison.Ordinal))
        {
            return BadRequest(new
            {
                message = "KeyId inválido."
            });
        }

        if (string.IsNullOrWhiteSpace(
                request.WrappedKey))
        {
            return BadRequest(new
            {
                message = "WrappedKey es requerido."
            });
        }

        byte[] wrappedKey;

        try
        {
            wrappedKey =
                Convert.FromBase64String(
                    request.WrappedKey);
        }
        catch (FormatException)
        {
            return BadRequest(new
            {
                message = "WrappedKey inválido."
            });
        }

        byte[] sessionKey;

        try
        {
            using var rsa = LoadPrivateKey();

            sessionKey =
                rsa.Decrypt(
                    wrappedKey,
                    RSAEncryptionPadding.OaepSHA256);
        }
        catch (CryptographicException)
        {
            return BadRequest(new
            {
                message = "No se pudo establecer la sesión."
            });
        }

        if (sessionKey.Length != 32)
        {
            CryptographicOperations.ZeroMemory(
                sessionKey);

            return BadRequest(new
            {
                message =
                    "La clave de sesión debe tener 256 bits."
            });
        }

        var session =
            _sessionService.CreateSession(
                sessionKey);

        return Ok(
            new CreateSecuritySessionResponse
            {
                SessionId = session.SessionId,
                KeyId = _options.KeyId,
                Algorithm = "A256GCM",
                ExpiresAt = session.ExpiresAt
            });
    }

    private RSA LoadPrivateKey()
    {
        if (string.IsNullOrWhiteSpace(
                _options.PrivateKeyPath))
        {
            throw new InvalidOperationException(
                "No se configuró PayloadEncryption:PrivateKeyPath.");
        }

        var path = Path.Combine(
            _environment.ContentRootPath,
            _options.PrivateKeyPath);

        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException(
                "No se encontró la clave privada RSA.",
                path);
        }

        var pem =
            System.IO.File.ReadAllText(path);

        var rsa = RSA.Create();

        rsa.ImportFromPem(pem);

        return rsa;
    }




    [HttpPost("test")]
public IActionResult TestPayload(
    [FromBody] JsonElement request)
{
    return Ok(new
    {
        message = "Payload recibido correctamente.",
        received = request
    });
}
}