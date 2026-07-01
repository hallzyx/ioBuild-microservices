using IoBuild.Subscriptions.Domain.Model.Queries;
using IoBuild.Subscriptions.Domain.Repositories.Services;
using IoBuild.Subscriptions.Interfaces.REST.Assemblers;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PlansController : ControllerBase
{
    private readonly IPlanQueryService _queryService;

    public PlansController(IPlanQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlans([FromQuery] string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            var nameQuery = new GetPlanByNameQuery(name);
            var plan = await _queryService.Handle(nameQuery);
            if (plan is null)
                return NotFound();
            return Ok(PlanAssembler.ToResource(plan));
        }

        var query = new GetAllPlansQuery();
        var plans = await _queryService.Handle(query);
        var resources = plans.Select(PlanAssembler.ToResource);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlanById(int id)
    {
        var query = new GetPlanByIdQuery(id);
        var plan = await _queryService.Handle(query);

        if (plan is null)
            return NotFound();

        return Ok(PlanAssembler.ToResource(plan));
    }
}
