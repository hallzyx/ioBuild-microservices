using Microsoft.AspNetCore.Mvc;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Domain.Services;
using IoBuild.Analytics.Interfaces.REST.Assemblers;
using Swashbuckle.AspNetCore.Annotations;

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
