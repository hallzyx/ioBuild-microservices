using IoBuild.Devices.Application.DTOs;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Interfaces.REST.Resources;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Interfaces.REST;

/// <summary>
/// Owner-gated endpoints for custom device type management.
///
/// POST /api/v1/devices/types/custom — create a new custom type (Owner only).
/// GET  /api/v1/devices/types/custom?ownerId= — list owner's custom types (Owner only;
///   ownerId query param must match the JWT principal; returns 403 on mismatch).
///
/// OwnerId is ALWAYS resolved from HttpContext.Items["UserId"] (set by JwtAuthenticationMiddleware).
/// Any body field or query param that attempts to set ownerId is ignored or validated against it.
///
/// Role check: same pattern as DeviceActuationService — read HttpContext.Items["UserRole"] and
/// return 403 when role != "Owner". The [Authorize] filter guards unauthenticated callers (401).
/// </summary>
[ApiController]
[Route("api/v1/devices/types/custom")]
[Authorize]
public class CustomDeviceTypeController(
    CustomDeviceTypeCommandService commandService,
    CustomDeviceTypeQueryService queryService) : ControllerBase
{
    /// <summary>
    /// POST /api/v1/devices/types/custom
    /// Creates a new owner-scoped custom device type.
    /// Returns 201 on success, 400 on domain validation failure, 403 on non-Owner role,
    /// 409 on duplicate (typeCode already exists for this owner).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCustomType([FromBody] CreateCustomTypeResource resource)
    {
        // ── 1. Auth check (Items["User"] set by [Authorize] filter / middleware) ──
        if (!HttpContext.Items.TryGetValue("UserId", out var rawUserId) || rawUserId is not int userId)
            return Unauthorized(new { error = "Authentication required." });

        // ── 2. Role guard ─────────────────────────────────────────────────────────
        var role = HttpContext.Items.TryGetValue("UserRole", out var rawRole) ? rawRole?.ToString() : null;
        if (!string.Equals(role, "Owner", StringComparison.Ordinal))
            return StatusCode(403, new { error = "Only owners may create custom device types." });

        // ── 3. Map resource → domain attributes ──────────────────────────────────
        IReadOnlyList<OwnerCustomDeviceTypeAttribute> attributes;
        try
        {
            attributes = resource.Attributes
                .Select(a => new OwnerCustomDeviceTypeAttribute(a.Name, a.Type, a.Min, a.Max, a.Unit, a.EnumMembers))
                .ToList();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        // ── 4. Persist (domain guards throw ArgumentException; unique index → DbUpdateException) ──
        CustomDeviceTypeDto dto;
        try
        {
            dto = await commandService.CreateAsync(
                ownerId: userId.ToString(),
                typeCode: resource.TypeCode,
                displayName: resource.DisplayName,
                attributes: attributes);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "A custom device type with this typeCode already exists for your account." });
        }

        return CreatedAtAction(nameof(ListCustomTypes), new { ownerId = dto.OwnerUserId }, dto);
    }

    /// <summary>
    /// GET /api/v1/devices/types/custom?ownerId={ownerId}
    /// Lists all custom device types for the authenticated owner.
    /// Returns 403 when the requested ownerId differs from the JWT principal.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListCustomTypes([FromQuery] string ownerId)
    {
        // ── 1. Auth ──────────────────────────────────────────────────────────────
        if (!HttpContext.Items.TryGetValue("UserId", out var rawUserId) || rawUserId is not int userId)
            return Unauthorized(new { error = "Authentication required." });

        // ── 2. Role guard ─────────────────────────────────────────────────────────
        var role = HttpContext.Items.TryGetValue("UserRole", out var rawRole) ? rawRole?.ToString() : null;
        if (!string.Equals(role, "Owner", StringComparison.Ordinal))
            return StatusCode(403, new { error = "Only owners may list custom device types." });

        // ── 3. Ownership guard: requested ownerId must match the token ────────────
        if (!string.Equals(ownerId, userId.ToString(), StringComparison.Ordinal))
            return StatusCode(403, new { error = "You may only list your own custom device types." });

        var result = await queryService.ListByOwnerAsync(userId.ToString());
        return Ok(result);
    }
}
