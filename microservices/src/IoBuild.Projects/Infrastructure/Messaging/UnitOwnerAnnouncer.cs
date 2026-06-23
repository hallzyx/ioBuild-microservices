using IoBuild.Projects.Domain.Repositories;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoBuild.Projects.Infrastructure.Messaging;

/// <summary>
/// Startup background service that re-publishes a <see cref="UnitOwnerMatchedEvent"/> for every
/// unit that already has an OwnerId in the DB, so the Devices service
/// <c>unit_owner_projections</c> table is repopulated after a full volume wipe
/// (<c>docker compose down -v</c>).
///
/// Design (mirrors DeviceRegistryAnnouncer in IoBuild.Devices):
/// - BackgroundService + IServiceScopeFactory so the scoped IUnitRepository is resolved safely.
/// - Publishes DIRECTLY via IDomainEventPublisher — NOT via the outbox — because the downstream
///   consumer (<c>UnitOwnerProjectionConsumer</c>) is idempotent (upsert by UnitId), making
///   repeated announcements on every restart safe by design, and avoiding outbox row accumulation.
/// - Exchange: <c>iobuild.domain.events</c> (topic, durable) — set by RabbitMqDomainEventPublisher.
/// - Routing key: <c>project.unit.owner-matched</c> — taken from <see cref="UnitOwnerMatchedEvent.RoutingKey"/>.
/// - AMQP header <c>event-type: UnitOwnerMatchedEvent</c> — set by RabbitMqDomainEventPublisher.
/// - <see cref="AnnounceAllAsync"/> is <c>internal</c> for unit-test access without a live broker.
/// </summary>
public class UnitOwnerAnnouncer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDomainEventPublisher _publisher;
    private readonly ILogger<UnitOwnerAnnouncer> _logger;

    public UnitOwnerAnnouncer(
        IServiceScopeFactory scopeFactory,
        IDomainEventPublisher publisher,
        ILogger<UnitOwnerAnnouncer> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await AnnounceAllAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnitOwnerAnnouncer: failed to re-announce unit-owner links on startup.");
        }
    }

    /// <summary>
    /// Queries all units with a non-null OwnerId and publishes a <see cref="UnitOwnerMatchedEvent"/>
    /// for each. Exposed as <c>internal</c> so tests can call it directly without hosting overhead.
    /// </summary>
    internal async Task AnnounceAllAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUnitRepository>();

        var units = (await repo.ListAsync())
            .Where(u => u.OwnerId.HasValue)
            .ToList();

        foreach (var unit in units)
        {
            var evt = new UnitOwnerMatchedEvent
            {
                UnitId      = unit.Id,
                ProjectId   = unit.ProjectId,
                OwnerUserId = unit.OwnerId!.Value,
                OwnerEmail  = unit.OwnerEmail ?? string.Empty
            };

            await _publisher.PublishAsync(evt, ct);
        }

        _logger.LogInformation(
            "UnitOwnerAnnouncer: re-announced {Count} unit-owner link(s) to exchange iobuild.domain.events.",
            units.Count);
    }
}
