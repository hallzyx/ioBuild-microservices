using Microsoft.AspNetCore.Mvc;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Domain.Services;
using IoBuild.Analytics.Interfaces.REST.Assemblers;
using Swashbuckle.AspNetCore.Annotations;
using IoBuild.Analytics.Infrastructure.InfluxDB;

namespace IoBuild.Analytics.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsQueryService _analyticsQueryService;

    public AnalyticsController(IAnalyticsQueryService analyticsQueryService)
    {
        _analyticsQueryService = analyticsQueryService;
    }

    [HttpGet("builders/{userId}/metrics")]
    [SwaggerOperation(Summary = "Gets builder dashboard metrics", Description = "Returns builder dashboard metrics for the specified user")]
    [SwaggerResponse(200, "Dashboard metrics retrieved successfully")]
    [SwaggerResponse(404, "User not found or no data available")]
    public async Task<IActionResult> GetBuilderDashboardMetrics(int userId)
    {
        var query = new GetBuilderDashboardQuery(userId);
        var result = await _analyticsQueryService.Handle(query);
        if (result == null)
            return NotFound(new { message = "No builder metrics found for the specified user." });
        return Ok(BuilderDashboardAssembler.ToResource(result));
    }

    [HttpGet("owners/{userId}/metrics")]
    [SwaggerOperation(Summary = "Gets owner dashboard metrics", Description = "Returns owner dashboard metrics for the specified user")]
    [SwaggerResponse(200, "Dashboard metrics retrieved successfully")]
    [SwaggerResponse(404, "User not found or no data available")]
    public async Task<IActionResult> GetOwnerDashboardMetrics(int userId)
    {
        var query = new GetOwnerDashboardQuery(userId);
        var result = await _analyticsQueryService.Handle(query);
        if (result == null)
            return NotFound(new { message = "No owner metrics found for the specified user." });
        return Ok(OwnerDashboardAssembler.ToResource(result));
    }

    [HttpGet("builders/{userId}/energy")]
    [SwaggerOperation(Summary = "Gets live energy consumption for a builder", Description = "Returns per-minute aggregated energy_kwh across all devices in the builder's projects")]
    [SwaggerResponse(200, "Live energy data retrieved successfully")]
    public async Task<IActionResult> GetBuilderLiveEnergy(int userId, [FromQuery] int minutes = 10)
    {
        var query = new GetBuilderLiveEnergyQuery(userId, Math.Clamp(minutes, 1, 60));
        var result = await _analyticsQueryService.Handle(query);
        return Ok(result.Select(p => new { timestamp = p.Timestamp, totalEnergyKwh = p.TotalEnergyKwh }));
    }

    [HttpGet("owners/{userId}/energy")]
    [SwaggerOperation(Summary = "Gets live energy consumption for an owner", Description = "Returns per-minute aggregated energy_kwh across all devices in units owned by the user")]
    [SwaggerResponse(200, "Live energy data retrieved successfully")]
    public async Task<IActionResult> GetOwnerLiveEnergy(int userId, [FromQuery] int minutes = 10)
    {
        var query = new GetOwnerLiveEnergyQuery(userId, Math.Clamp(minutes, 1, 60));
        var result = await _analyticsQueryService.Handle(query);
        return Ok(result.Select(p => new { timestamp = p.Timestamp, totalEnergyKwh = p.TotalEnergyKwh }));
    }

    [HttpGet("insights")]
    [SwaggerOperation(Summary = "Gets historical data insights", Description = "Returns time-series data for a given metric within a date range")]
    [SwaggerResponse(200, "Historical data retrieved successfully")]
    public async Task<IActionResult> GetHistoricalData(
        [FromQuery] int projectId,
        [FromQuery] string metric = "temperature",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var query = new GetHistoricalDataQuery(projectId, metric, start, end);
        var result = await _analyticsQueryService.Handle(query);
        var resources = result.Select(HistoricalDataPointAssembler.ToResource);
        return Ok(resources);
    }
}
