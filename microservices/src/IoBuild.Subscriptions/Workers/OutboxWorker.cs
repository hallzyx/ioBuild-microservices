using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Infrastructure.Persistence.EFC;

namespace IoBuild.Subscriptions.Workers;

public class OutboxWorker : BackgroundService
{
    private const int MaxRetries = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
                var context = scope.ServiceProvider.GetRequiredService<SubscriptionsDbContext>();

                var pending = await outboxRepo.GetPendingAsync();
                foreach (var msg in pending)
                {
                    try
                    {
                        _logger.LogInformation("Processing outbox message {MessageId}: {EventType}", msg.Id, msg.EventType);
                        msg.Status = "Processed";
                        msg.ProcessedAt = DateTime.UtcNow;
                        await outboxRepo.UpdateAsync(msg);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process outbox message {MessageId}", msg.Id);
                        msg.RetryCount++;
                        msg.Error = ex.Message;
                        if (msg.RetryCount >= MaxRetries) msg.Status = "Dead";
                        await outboxRepo.UpdateAsync(msg);
                    }
                }

                if (pending.Count > 0)
                {
                    await context.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxWorker cycle error");
            }

            await Task.Delay(5000, stoppingToken);
        }

        _logger.LogInformation("OutboxWorker stopped");
    }
}
