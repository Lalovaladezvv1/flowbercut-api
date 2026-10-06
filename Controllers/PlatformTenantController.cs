using System.Security.Claims;

using Flowbercut.Api.Models.Requests;
using Flowbercut.Api.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flowbercut.Api.Controllers;

[ApiController]
[Authorize(Roles = "PLATFORM_ADMIN")]
[Route("api/v1/platform/barberias")]
public sealed class PlatformTenantController
    : ControllerBase
{
    private readonly IPlatformTenantService
        _tenantService;

    public PlatformTenantController(
        IPlatformTenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerBarberias(
        CancellationToken cancellationToken)
    {
        var barberias =
            await _tenantService.ObtenerBarberiasAsync(
                cancellationToken);

        return Ok(barberias);
    }

    [HttpPost]
    public async Task<IActionResult> CrearBarberia(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var barberia =
            await _tenantService.CrearBarberiaAsync(
                request,
                usuarioId.Value,
                cancellationToken);

        if (barberia is null)
        {
            return BadRequest();
        }

        return Created(
            $"/api/v1/platform/barberias/{barberia.Id}",
            barberia);
    }

    [HttpPost("{tenantId:long}/administrador")]
    public async Task<IActionResult> CrearAdministrador(
        long tenantId,
        [FromBody] CreateTenantAdminRequest request,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var administrador =
            await _tenantService.CrearAdministradorAsync(
                tenantId,
                request,
                usuarioId.Value,
                cancellationToken);

        if (administrador is null)
        {
            return BadRequest();
        }

        return Ok(administrador);
    }

    [HttpPut("{tenantId:long}/administrador/contrasena")]
    public async Task<IActionResult>
        RestablecerContrasenaAdministrador(
            long tenantId,
            [FromBody]
            ResetTenantAdminPasswordRequest request,
            CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var administrador =
            await _tenantService
                .RestablecerContrasenaAdministradorAsync(
                    tenantId,
                    request,
                    usuarioId.Value,
                    cancellationToken);

        if (administrador is null)
        {
            return BadRequest();
        }

        return Ok(administrador);
    }

    [HttpPatch("{tenantId:long}/estatus")]
    public async Task<IActionResult>
        CambiarEstatus(
            long tenantId,
            [FromBody] int estatusId,
            CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized();
        }

        var barberia =
            await _tenantService.CambiarEstatusAsync(
                tenantId,
                estatusId,
                usuarioId.Value,
                cancellationToken);

        if (barberia is null)
        {
            return BadRequest();
        }

        return Ok(barberia);
    }

    private long? ObtenerUsuarioId()
    {
        var usuarioIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return long.TryParse(
            usuarioIdClaim,
            out var usuarioId)
            ? usuarioId
            : null;
    }
}