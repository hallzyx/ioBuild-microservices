using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Domain.Services.Queries.Projects;
using IoBuild.Projects.Interfaces.Resources;
using IoBuild.Projects.Interfaces.Transform;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Projects.Interfaces.REST;

[ApiController]
[Route("api/v1/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectCommandService _commandService;
    private readonly IProjectQueryService _queryService;

    public ProjectsController(IProjectCommandService commandService, IProjectQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _queryService.Handle(new GetAllProjectsQuery());
        var resources = ProjectResourceFromEntityAssembler.ToResourceEnumerable(projects);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _queryService.Handle(new GetProjectByIdQuery(id));
        if (project == null)
            return NotFound();

        var resource = ProjectResourceFromEntityAssembler.ToResource(project);
        return Ok(resource);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectResource resource)
    {
        var command = CreateProjectCommandFromResourceAssembler.ToCommand(resource);
        var id = await _commandService.Handle(command);
        var project = await _queryService.Handle(new GetProjectByIdQuery(id));
        var result = ProjectResourceFromEntityAssembler.ToResource(project!);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectResource resource)
    {
        var command = UpdateProjectCommandFromResourceAssembler.ToCommand(id, resource);
        await _commandService.Handle(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _commandService.Handle(new DeleteProjectCommand(id));
        return NoContent();
    }
}
