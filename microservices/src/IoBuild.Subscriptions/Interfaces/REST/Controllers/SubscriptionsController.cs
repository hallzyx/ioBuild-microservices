using IoBuild.Subscriptions.Domain.Model.Queries;
using IoBuild.Subscriptions.Domain.Repositories.Services;
using IoBuild.Subscriptions.Interfaces.REST.Assemblers;
using IoBuild.Subscriptions.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionCommandService _commandService;
    private readonly ISubscriptionQueryService _queryService;

    public SubscriptionsController(
        ISubscriptionCommandService commandService,
        ISubscriptionQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSubscriptions()
    {
        var query = new GetAllSubscriptionsQuery();
        var subscriptions = await _queryService.Handle(query);
        var resources = subscriptions.Select(SubscriptionAssembler.ToResource);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubscriptionById(int id)
    {
        var query = new GetSubscriptionByIdQuery(id);
        var subscription = await _queryService.Handle(query);

        if (subscription is null)
            return NotFound();

        return Ok(SubscriptionAssembler.ToResource(subscription));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionResource resource)
    {
        var command = SubscriptionAssembler.ToCommand(resource);
        var subscription = await _commandService.Handle(command);
        return CreatedAtAction(nameof(GetSubscriptionById), new { id = subscription.Id },
            SubscriptionAssembler.ToResource(subscription));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubscription(int id, [FromBody] UpdateSubscriptionResource resource)
    {
        var command = SubscriptionAssembler.ToCommand(id, resource);
        var subscription = await _commandService.Handle(command);
        return Ok(SubscriptionAssembler.ToResource(subscription));
    }
}
