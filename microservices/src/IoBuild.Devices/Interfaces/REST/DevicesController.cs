using IoBuild.Devices.Domain.Constants;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Mvc;
using IoBuild.Devices.Domain.Model.Queries;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Interfaces.REST.Resources;
using IoBuild.Devices.Interfaces.REST.Transform;

namespace IoBuild.Devices.Interfaces.REST;

[ApiController]
[Route("api/v1/devices")]
[Authorize]
public class DevicesController(IDeviceCommandService commandService, IDeviceQueryService queryService) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/devices/types
    /// Returns the device-type catalog used for per-floor provisioning (S1.1, SC-1.1).
    /// No authentication required (spec S8 — "No new auth").
    /// Reads from FloorDeviceDefaults.Catalog — the intended named accessor for this
    /// endpoint (SG1 / W3 fix). Catalog projects Defaults to (Type, DisplayName) tuples.
    /// Alignment with DeviceTypeCatalog.KnownTypes is test-asserted in
    /// DeviceTypeCatalogControllerTests.GetDeviceTypes_Codes_MatchSharedDeviceTypeCatalogKnownTypes.
    /// </summary>
    [HttpGet("types")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult GetDeviceTypes()
    {
        var entries = FloorDeviceDefaults.Catalog
            .Select(d => new DeviceTypeResource(d.Type, d.DisplayName))
            .ToList();
        return Ok(new DeviceTypeCatalogResource(entries));
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
