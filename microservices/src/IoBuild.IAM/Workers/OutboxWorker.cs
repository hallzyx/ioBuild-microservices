using System.Text.Json;
using IoBuild.IAM.Domain.Model.Entities;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Services;
using IoBuild.Shared.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace IoBuild.IAM.Workers;

/// <summary>
/// Background service that polls the IAM outbox table and publishes pending events
/// to RabbitMQ (ADR-2, ADR-E).
///
/// EventTypeMap contains only <see cref="UserRegisteredEvent"/> (§2.1, §6.3).
/// Design mirrors IoBuild.Projects.Workers.OutboxWorker exactly.
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDomainEventPublisher _publisher;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly int _pollIntervalMs;

    private const int MaxRetries = 5;

    // IAM only publishes UserRegisteredEvent (§6.3 routing-key table).
    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        [nameof(UserRegisteredEvent)] = typeof(UserRegisteredEvent),
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
        _logger.LogInformation("IAM OutboxWorker starting. Poll interval: {PollMs}ms", _pollIntervalMs);

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
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IIamOutboxMessageRepository>();

        List<OutboxMessage> pending;
        try
        {
            pending = await outboxRepo.GetPendingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IAM OutboxWorker: failed to query pending messages");
            return;
        }

        if (pending.Count == 0)
            return;

        foreach (var msg in pending)
        {
            if (msg.RetryCount >= MaxRetries)
            {
                msg.Status = "Dead";
                _logger.LogError(
                    "IAM OutboxWorker: message id={Id} EventType={EventType} exceeded {Max} retries. Quarantining.",
                    msg.Id, msg.EventType, MaxRetries);
                try { await outboxRepo.UpdateAsync(msg); } catch { }
                continue;
            }

            try
            {
                var domainEvent = DeserializeEvent(msg);
                if (domainEvent is null)
                {
                    _logger.LogWarning(
                        "IAM OutboxWorker: unknown EventType={EventType} for OutboxMessage id={Id}. Skipping.",
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
                    "IAM OutboxWorker: published {EventType} EventId={EventId}",
                    msg.EventType, msg.EventId);
            }
            catch (Exception ex)
            {
                msg.RetryCount++;
                msg.Error = ex.Message;
                _logger.LogWarning(
                    ex,
                    "IAM OutboxWorker: publish failed for {EventType} id={Id}. RetryCount={Retry}",
                    msg.EventType, msg.Id, msg.RetryCount);
            }

            try
            {
                await outboxRepo.UpdateAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IAM OutboxWorker: failed to update OutboxMessage id={Id}", msg.Id);
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
