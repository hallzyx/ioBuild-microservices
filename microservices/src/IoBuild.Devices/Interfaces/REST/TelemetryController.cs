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
public class TelemetryController(
    IDeviceQueryService deviceQueryService,
    ITelemetryQueryService telemetryQueryService) : ControllerBase
{
    /// <summary>
    /// Obtiene datos de energía de un dispositivo en un rango de tiempo.
    /// </summary>
    [HttpGet("{id}/energy")]
    public async Task<IActionResult> GetDeviceEnergy(
        int id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var device = await deviceQueryService.Handle(new GetDeviceByIdQuery(id));
        if (device is null)
            return NotFound(new { message = $"Device with ID {id} not found" });

        var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
        var toDate = to ?? DateTime.UtcNow;

        var query = new GetDeviceEnergyQuery(id, fromDate, toDate);
        var dataPoints = await telemetryQueryService.Handle(query);

        var resources = dataPoints.Select(TelemetryResourceAssembler.ToEnergyResource);
        return Ok(resources);
    }

    /// <summary>
    /// Obtiene el estado actual de un dispositivo.
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetDeviceStatus(int id)
    {
        var device = await deviceQueryService.Handle(new GetDeviceByIdQuery(id));
        if (device is null)
            return NotFound(new { message = $"Device with ID {id} not found" });

        var query = new GetDeviceStatusQuery(id);
        var report = await telemetryQueryService.Handle(query);

        if (report is null)
        {
            var unknownResource = new DeviceStatusResource(id, "unknown", DateTime.MinValue, 0, 0);
            return Ok(unknownResource);
        }

        var resource = TelemetryResourceAssembler.ToStatusResource(report);
        return Ok(resource);
    }
}
