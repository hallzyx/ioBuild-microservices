using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Units;
using IoBuild.Projects.Domain.Services.Queries.Units;
using IoBuild.Projects.Interfaces.Resources;
using IoBuild.Projects.Interfaces.Transform;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Projects.Interfaces.REST;

[ApiController]
[Route("api/v1/units")]
[Authorize]
public class UnitsController : ControllerBase
{
    private readonly IUnitCommandService _commandService;
    private readonly IUnitQueryService _queryService;

    public UnitsController(IUnitCommandService commandService, IUnitQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var units = await _queryService.Handle(new GetAllUnitsQuery());
        var resources = UnitResourceFromEntityAssembler.ToResourceEnumerable(units);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var unit = await _queryService.Handle(new GetUnitByIdQuery(id));
        if (unit == null)
            return NotFound();

        var resource = UnitResourceFromEntityAssembler.ToResource(unit);
        return Ok(resource);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitResource resource)
    {
        var command = CreateUnitCommandFromResourceAssembler.ToCommand(resource);
        var id = await _commandService.Handle(command);
        var unit = await _queryService.Handle(new GetUnitByIdQuery(id));
        var result = UnitResourceFromEntityAssembler.ToResource(unit!);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Soft delete not implemented; remove all related data or mark as inactive.
        return Ok(new { message = $"Unit {id} deletion is not yet implemented." });
    }
}
