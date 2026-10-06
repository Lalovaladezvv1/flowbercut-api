using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Flowbercut.Api.Models.Security;
using Flowbercut.Api.Security;

namespace Flowbercut.Api.Middleware;

public sealed class PayloadEncryptionMiddleware
{
    private const string SessionHeader = "X-Flow-Session-Id";
    private const string RequestIdHeader = "X-Request-Id";

    private readonly RequestDelegate _next;
    private readonly ISecuritySessionService _sessionService;
    private readonly IPayloadEncryptionService _encryptionService;
    private readonly ILogger<PayloadEncryptionMiddleware> _logger;

    public PayloadEncryptionMiddleware(
        RequestDelegate next,
        ISecuritySessionService sessionService,
        IPayloadEncryptionService encryptionService,
        ILogger<PayloadEncryptionMiddleware> logger)
    {
        _next = next;
        _sessionService = sessionService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        if (!TryGetSession(
                context,
                out var session) ||
            session is null)
        {
            await WriteError(
                context,
                StatusCodes.Status401Unauthorized,
                "Sesión de cifrado inválida.");

            return;
        }

        var requestId =
            context.Request.Headers[RequestIdHeader]
                .FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.Items["FlowbercutRequestId"] = requestId;

        context.Response.Headers[RequestIdHeader] = requestId;

        try
        {
            await DecryptRequest(
                context,
                session);

            var originalBody = context.Response.Body;

            await using var responseStream =
                new MemoryStream();

            context.Response.Body = responseStream;

            try
            {
                await _next(context);

                await EncryptResponse(
                    context,
                    session,
                    requestId,
                    responseStream,
                    originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
        catch (CryptographicException ex)
        {
            _logger.LogWarning(
                ex,
                "Payload cifrado inválido. RequestId: {RequestId}",
                requestId);

            await WriteError(
                context,
                StatusCodes.Status400BadRequest,
                "Payload inválido.");
        }
    }

    private async Task DecryptRequest(
        HttpContext context,
        SecuritySession session)
    {
        if (context.Request.ContentLength is null ||
            context.Request.ContentLength == 0)
        {
            return;
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        var envelope =
            JsonSerializer.Deserialize<PayloadEnvelope>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (envelope is null)
        {
            throw new CryptographicException(
                "No se pudo interpretar el envelope.");
        }

        if (!string.Equals(
                envelope.SessionId,
                session.SessionId,
                StringComparison.Ordinal))
        {
            throw new CryptographicException(
                "La sesión del payload no coincide.");
        }

        var requestId =
            context.Items["FlowbercutRequestId"]?.ToString()
            ?? throw new CryptographicException(
                "RequestId no encontrado.");

        if (!string.Equals(
                envelope.RequestId,
                requestId,
                StringComparison.Ordinal))
        {
            throw new CryptographicException(
                "RequestId inválido.");
        }

        /*
         * Replay protection.
         *
         * El RequestId solamente puede utilizarse una vez
         * dentro de la misma sesión.
         */
        if (!_sessionService.TryRegisterRequest(
                session.SessionId,
                requestId))
        {
            _logger.LogWarning(
                "REPLAY DETECTADO. SessionId: {SessionId}, RequestId: {RequestId}",
                session.SessionId,
                requestId);

            throw new CryptographicException(
                "El request ya fue procesado.");
        }

        _logger.LogInformation(
            "REQUEST REGISTRADO. SessionId: {SessionId}, RequestId: {RequestId}",
            session.SessionId,
            requestId);

        var plaintext =
            _encryptionService.Decrypt(
                envelope,
                session,
                context.Request.Method,
                context.Request.Path);

        var plaintextBytes =
            Encoding.UTF8.GetBytes(plaintext);

        context.Request.Body =
            new MemoryStream(plaintextBytes);

        context.Request.ContentLength =
            plaintextBytes.Length;

        context.Request.ContentType =
            "application/json";
    }

    private async Task EncryptResponse(
        HttpContext context,
        SecuritySession session,
        string requestId,
        MemoryStream responseStream,
        Stream originalBody)
    {
        responseStream.Position = 0;

        var responseBody =
            await new StreamReader(
                responseStream,
                Encoding.UTF8)
            .ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return;
        }

        var envelope =
            _encryptionService.Encrypt(
                responseBody,
                session,
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);

        var json =
            JsonSerializer.Serialize(
                envelope,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase
                });

        var encryptedBytes =
            Encoding.UTF8.GetBytes(json);

        context.Response.ContentType =
            "application/json";

        context.Response.ContentLength =
            encryptedBytes.Length;

        await originalBody.WriteAsync(
            encryptedBytes);
    }

    private bool TryGetSession(
        HttpContext context,
        out SecuritySession? session)
    {
        session = null;

        var sessionId =
            context.Request.Headers[SessionHeader]
                .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return false;
        }

        return _sessionService.TryGetSession(
            sessionId,
            out session);
    }

    private static bool ShouldSkip(
        HttpContext context)
    {
        var path = context.Request.Path;

        return
            path.StartsWithSegments(
                "/api/v1/security/public-key") ||
            path.StartsWithSegments(
                "/api/v1/security/session") ||
            path.StartsWithSegments(
                "/swagger") ||
            path.StartsWithSegments(
                "/health");
    }

    private static async Task WriteError(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;

        context.Response.ContentType =
            "application/json";

        await context.Response.WriteAsJsonAsync(
            new
            {
                message
            });
    }
}