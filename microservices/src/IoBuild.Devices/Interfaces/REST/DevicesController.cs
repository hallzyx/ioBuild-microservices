using IoBuild.Devices.Domain.Constants;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Interfaces.REST.Resources;
using IoBuild.Devices.Interfaces.REST.Transform;
using IoBuild.Shared.Domain.Model;
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
    IDeviceActuationService? actuationService = null) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/devices/types
    /// Returns the full device-type catalog (floor-level + unit-level = 5 types) with
    /// controllable attributes populated from DeviceCapabilityCatalog (R-3, task 3.3).
    /// Telemetry-only types return an empty ControllableAttributes list.
    /// No authentication required (spec S8).
    /// </summary>
    [HttpGet("types")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult GetDeviceTypes()
    {
        var entries = FloorDeviceDefaults.Catalog
            .Concat(UnitDeviceCatalog.Catalog)
            .Select(d =>
            {
                // Populate ControllableAttributes from DeviceCapabilityCatalog (R-3)
                IReadOnlyList<ControllableAttributeResource> controllableAttrs = [];

                if (DeviceCapabilityCatalog.ByType.TryGetValue(d.Type, out var capability))
                {
                    controllableAttrs = capability.ControllableAttributes
                        .Select(a => new ControllableAttributeResource(a.Name, a.Type, a.Min, a.Max, a.Unit, a.EnumMembers))
                        .ToList();
                }

                return new DeviceTypeResource(d.Type, d.DisplayName, controllableAttrs);
            })
            .ToList();

        return Ok(new DeviceTypeCatalogResource(entries));
    }

    /// <summary>
    /// POST /api/v1/devices/{id}/command
    /// Owner-gated endpoint to send a command to a controllable device (D-3, ADR-B5).
    /// Reads UserId + UserRole from HttpContext.Items (set by JWT middleware).
    /// Returns 200 on success, 400 on invalid attribute/range, 403 on missing ownership,
    /// 404 on missing device.
    /// </summary>
    [HttpPost("{id}/command")]
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
        var command = DeviceResourceToCommandAssembler.ToCommandFromResource(resource);
        var device = await commandService.Handle(command);
        var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
        return CreatedAtAction(nameof(GetDeviceById), new { id = deviceResource.Id }, deviceResource);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, [FromBody] UpdateDeviceResource resource)
    {
        var command = DeviceResourceToCommandAssembler.ToCommandFromResource(id, resource);
        var device = await commandService.Handle(command);
        var deviceResource = DeviceResourceFromEntityAssembler.ToResourceFromEntity(device);
        return Ok(deviceResource);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var command = new Domain.Model.Commands.DeleteDeviceCommand(id);
        await commandService.Handle(command);
        return NoContent();
    }
}
