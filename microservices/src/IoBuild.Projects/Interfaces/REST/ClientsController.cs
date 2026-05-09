using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Domain.Services.Commands.Clients;
using IoBuild.Projects.Domain.Services.Queries.Clients;
using IoBuild.Projects.Interfaces.Resources;
using IoBuild.Projects.Interfaces.Transform;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Projects.Interfaces.REST;

[ApiController]
[Route("api/v1/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientCommandService _commandService;
    private readonly IClientQueryService _queryService;

    public ClientsController(IClientCommandService commandService, IClientQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _queryService.Handle(new GetAllClientsQuery());
        var resources = ClientResourceFromEntityAssembler.ToResourceEnumerable(clients);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _queryService.Handle(new GetClientByIdQuery(id));
        if (client == null)
            return NotFound();

        var resource = ClientResourceFromEntityAssembler.ToResource(client);
        return Ok(resource);
    }

    [HttpGet("by-project/{projectId}")]
    public async Task<IActionResult> GetByProjectId(int projectId)
    {
        var clients = await _queryService.Handle(new GetClientsByProjectIdQuery(projectId));
        var resources = ClientResourceFromEntityAssembler.ToResourceEnumerable(clients);
        return Ok(resources);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientResource resource)
    {
        var command = CreateClientCommandFromResourceAssembler.ToCommand(resource);
        var id = await _commandService.Handle(command);
        var client = await _queryService.Handle(new GetClientByIdQuery(id));
        var result = ClientResourceFromEntityAssembler.ToResource(client!);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientResource resource)
    {
        var command = UpdateClientCommandFromResourceAssembler.ToCommand(id, resource);
        await _commandService.Handle(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _commandService.Handle(new DeleteClientCommand(id));
        return NoContent();
    }
}
