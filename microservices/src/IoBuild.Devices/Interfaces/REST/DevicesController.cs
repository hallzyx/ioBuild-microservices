using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Exceptions;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Interfaces.REST.Resources;
using IoBuild.Devices.Interfaces.REST.Transform;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Mvc;
using IoBuild.Devices.Domain.Model.Queries;

namespace IoBuild.Devices.Interfaces.REST;

[ApiController]
[Route("api/v1/devices")]
[Authorize]
public class DevicesController(
    IDeviceCommandService commandService,
    IDeviceQueryService queryService,
    DeviceTypeCatalogQueryService? catalogQueryService = null,
    IDeviceActuationService? actuationService = null) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/devices/types
    /// Returns all device types from the DB catalog (design D5).
    /// Response shape is field-for-field compatible with the previous static implementation:
    ///   { deviceTypes: [ { code, displayName, controllableAttributes } ] }
    /// Scope is NOT surfaced in the response (zero frontend change per design D5).
    /// No authentication required (AllowAnonymous).
    /// </summary>
    [HttpGet("types")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> GetDeviceTypes()
    {
        if (catalogQueryService is null)
            return StatusCode(503, "Device type catalog service not configured.");

        var entries = await catalogQueryService.ListAllAsync();
        return Ok(new DeviceTypeCatalogResource(entries));
    }

    /// <summary>
    /// POST /api/v1/devices/{id}/command
    /// Owner-gated endpoint to send a command to a controllable device (D-3, ADR-B5).
    /// Reads UserId + UserRole from HttpContext.Items (set by JWT middleware).
    /// Returns 200 on success, 400 on invalid attribute/range, 403 on missing ownership,
    /// 404 on missing device.
    /// </summary>
    [HttpPost("{id}/commands")]
    public async Task<IActionResult> SendDeviceCommand(int id, [FromBody] SendCommandResource resource)
    {
        if (actuationService is null)
            return StatusCode(503, "Actuation service not configured.");

        var userId = HttpContext.Items.TryGetValue("UserId", out var rawUserId) && rawUserId is int uid
            ? uid
            : 0;

        var userRole = HttpContext.Items.TryGetValue("UserRole", out var rawRole)
            ? rawRole?.ToString()
            : null;

        var command = new SendDeviceCommandCommand(
            DeviceId: id,
            Attribute: resource.Attribute,
            Value: resource.Value,
            RequestingUserId: userId,
            RequestingUserRole: userRole);

        var result = await actuationService.Handle(command);

        return result.StatusCode switch
        {
            200 => Ok(new CommandResultResource(id, resource.Attribute, resource.Value, result.AcceptedAt!.Value)),
            400 => BadRequest(new { error = result.ErrorMessage }),
            403 => StatusCode(403, new { error = result.ErrorMessage }),
            404 => NotFound(new { error = result.ErrorMessage }),
            _   => StatusCode(500, new { error = "Unexpected error." })
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDevices()
    {
        var query = new GetAllDevicesQuery();
        var devices = await queryService.Handle(query);
        var resources = devices.Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeviceById(int id)
    {
        var query = new GetDeviceByIdQuery(id);
        var device = await queryService.Handle(query);

        if (device is null)
            return NotFound();

        var resource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
        return Ok(resource);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceResource resource)
    {
        // Resolve requesting owner id from JWT context (used when UnitId is set)
        var requestingOwnerId = HttpContext.Items.TryGetValue("UserId", out var rawId) && rawId is int uid
            ? uid.ToString()
            : null;

        var command = DeviceResourceToCommandAssembler.ToCommandFromResource(resource, requestingOwnerId);

        try
        {
            var device = await commandService.Handle(command);
            var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
            return CreatedAtAction(nameof(GetDeviceById), new { id = deviceResource.Id }, deviceResource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (DuplicateDeviceTypeOnUnitException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, [FromBody] UpdateDeviceResource resource)
    {
        var command = DeviceResourceToCommandAssembler.ToCommandFromResource(id, resource);
        await commandService.Handle(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var command = new Domain.Model.Commands.DeleteDeviceCommand(id);
        await commandService.Handle(command);
        return NoContent();
    }
}
