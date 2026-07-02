using System.Text.Json;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
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
    ITelemetryQueryService telemetryQueryService,
    DevicesDbContext db) : ControllerBase
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
    /// Obtiene el estado actual de un dispositivo, incluyendo el estado deseado del shadow.
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetDeviceStatus(int id)
    {
        var device = await deviceQueryService.Handle(new GetDeviceByIdQuery(id));
        if (device is null)
            return NotFound(new { message = $"Device with ID {id} not found" });

        var statusTask = telemetryQueryService.Handle(new GetDeviceStatusQuery(id));
        var shadow = await db.DeviceShadows.FindAsync(id);
        var report = await statusTask;

        Dictionary<string, object>? desired = null;
        if (shadow?.DesiredJson is not null && shadow.DesiredJson != "{}")
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(shadow.DesiredJson);
            if (raw is not null)
            {
                desired = raw.ToDictionary(
                    kv => kv.Key,
                    kv => (object)(kv.Value.ValueKind switch
                    {
                        JsonValueKind.Number when kv.Value.TryGetDouble(out var d) => (object)d,
                        JsonValueKind.True  => (object)true,
                        JsonValueKind.False => (object)false,
                        _                  => kv.Value.GetString() ?? kv.Value.GetRawText()
                    }));
            }
        }

        if (report is null)
        {
            var unknownResource = new DeviceStatusResource(id, "unknown", DateTime.MinValue, 0, 0, desired);
            return Ok(unknownResource);
        }

        var resource = TelemetryResourceAssembler.ToStatusResource(report, desired);
        return Ok(resource);
    }
}
