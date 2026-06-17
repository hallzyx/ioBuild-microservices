using System.Text.Json;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Services;
using IoBuild.Shared.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace IoBuild.Projects.Workers;

/// <summary>
/// Background service that polls the Projects outbox table and publishes pending events
/// to RabbitMQ (ADR-2, REQ-DE-03, REQ-DE-06).
///
/// Design:
/// - Polls every ~5 s (configurable via <c>OutboxWorker:PollIntervalMs</c> in config).
/// - For each pending row: Polly-wrapped publish → mark Processed on success, RetryCount++ on failure.
/// - Circuit breaker lives in the Polly pipeline resolved from DI (not embedded in the publisher).
/// - Exceptions from the Polly pipeline (incl. BrokenCircuitException) are caught here; the worker
///   never propagates unhandled exceptions — rows stay Pending and are retried on the next cycle.
/// - The scoped <see cref="IOutboxMessageRepository"/> is resolved inside a fresh scope per cycle.
/// - <see cref="RunOneCycleAsync"/> is <c>public virtual</c> for unit-testing without hosting.
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDomainEventPublisher _publisher;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly int _pollIntervalMs;

    // Routing-key map from EventType string to DomainEvent deserialization type.
    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(ProjectCreatedEvent)] = typeof(ProjectCreatedEvent),
        [nameof(ProjectUpdatedEvent)] = typeof(ProjectUpdatedEvent),
        [nameof(UnitCreatedEvent)] = typeof(UnitCreatedEvent),
    };

    public OutboxWorker(
        IServiceScopeFactory scopeFactory,
        IDomainEventPublisher publisher,
        ILogger<OutboxWorker> logger,
        [FromKeyedServices(DomainEventPublishingExtensions.OutboxResiliencePipelineKey)]
        ResiliencePipeline? pipeline = null,
        int pollIntervalMs = 5_000)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
        _pipeline = pipeline ?? DomainEventPublishingExtensions.BuildResiliencePipeline();
        _pollIntervalMs = pollIntervalMs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Projects OutboxWorker starting. Poll interval: {PollMs}ms", _pollIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOneCycleAsync(stoppingToken);
            await Task.Delay(_pollIntervalMs, stoppingToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Exposed for unit testing: runs one poll-and-publish cycle.
    /// Must not throw — all exceptions are caught, logged, and reflected in RetryCount.
    /// </summary>
    public virtual async Task RunOneCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();

        List<OutboxMessage> pending;
        try
        {
            pending = await outboxRepo.GetPendingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Projects OutboxWorker: failed to query pending messages");
            return;
        }

        if (pending.Count == 0)
            return;

        foreach (var msg in pending)
        {
            try
            {
                var domainEvent = DeserializeEvent(msg);
                if (domainEvent is null)
                {
                    _logger.LogWarning(
                        "Projects OutboxWorker: unknown EventType={EventType} for OutboxMessage id={Id}. Skipping.",
                        msg.EventType, msg.Id);
                    continue;
                }

                await _pipeline.ExecuteAsync(async ct =>
                {
                    await _publisher.PublishAsync(domainEvent, ct);
                }, cancellationToken);

                msg.Status = "Processed";
                msg.ProcessedAt = DateTime.UtcNow;
                _logger.LogDebug(
                    "Projects OutboxWorker: published {EventType} EventId={EventId}",
                    msg.EventType, msg.EventId);
            }
            catch (Exception ex)
            {
                msg.RetryCount++;
                msg.Error = ex.Message;
                _logger.LogWarning(
                    ex,
                    "Projects OutboxWorker: publish failed for {EventType} id={Id}. RetryCount={Retry}",
                    msg.EventType, msg.Id, msg.RetryCount);
            }

            try
            {
                await outboxRepo.UpdateAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projects OutboxWorker: failed to update OutboxMessage id={Id}", msg.Id);
            }
        }
    }

    private static DomainEvent? DeserializeEvent(OutboxMessage msg)
    {
        if (!EventTypeMap.TryGetValue(msg.EventType, out var targetType))
            return null;

        return (DomainEvent?)JsonSerializer.Deserialize(msg.Payload, targetType);
    }
}
