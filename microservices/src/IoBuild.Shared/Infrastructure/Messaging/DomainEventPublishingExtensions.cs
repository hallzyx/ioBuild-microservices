using IoBuild.Shared.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;

namespace IoBuild.Shared.Infrastructure.Messaging;

/// <summary>
/// DI extension that registers the domain-event publishing infrastructure (ADR-8).
///
/// Registered services:
/// - <see cref="IConnection"/> (singleton) — long-lived AMQP connection, expensive to create.
/// - <see cref="IDomainEventPublisher"/> → <see cref="RabbitMqDomainEventPublisher"/> (singleton).
/// - <c>ResiliencePipeline</c> (keyed singleton, key: <see cref="OutboxResiliencePipelineKey"/>) —
///   the Polly circuit-breaker pipeline for use by the OutboxWorker. The pipeline is NOT embedded
///   in the publisher; the OutboxWorker resolves it from DI and wraps publisher calls itself (ADR-2).
///
/// Configuration key: <c>RabbitMq:ConnectionString</c>
/// e.g. <c>amqp://iobuild:iobuild@rabbitmq:5672/</c>
/// </summary>
public static class DomainEventPublishingExtensions
{
    /// <summary>Keyed DI key for the Polly <see cref="ResiliencePipeline"/> used by OutboxWorkers.</summary>
    public const string OutboxResiliencePipelineKey = "outbox-resilience-pipeline";

    /// <summary>
    /// Registers the RabbitMQ connection, publisher, and outbox resilience pipeline.
    /// </summary>
    public static IServiceCollection AddDomainEventPublishing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["RabbitMq:ConnectionString"]
            ?? "amqp://guest:guest@localhost:5672/";

        // Singleton AMQP connection — created lazily on first resolve.
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            // CreateConnectionAsync is async; we block here once at startup.
            // For a hosted service environment this is acceptable at registration time.
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        // Singleton publisher — owns no Polly pipeline, cleanly testable.
        services.AddSingleton<IDomainEventPublisher, RabbitMqDomainEventPublisher>();

        // Keyed singleton: the ADR-2 outbox resilience pipeline.
        services.AddKeyedSingleton<ResiliencePipeline>(
            OutboxResiliencePipelineKey,
            (_, _) => BuildResiliencePipeline());

        return services;
    }

    /// <summary>
    /// Builds the ADR-2 Polly resilience pipeline: inner retry (2×200ms) + circuit breaker.
    ///
    /// Thresholds (ADR-2):
    /// - FailureRatio = 0.5 — open when ≥50% of sampled publishes fail.
    /// - MinimumThroughput = 4 — require ≥4 attempts in the window before the ratio is evaluated.
    /// - SamplingDuration = 30s — rolling window for the ratio.
    /// - BreakDuration = 15s — time the circuit stays Open before moving to Half-Open.
    /// - Inner retry: 2 attempts, constant 200ms delay (handles transient blips before the breaker counts a failure).
    ///
    /// The pipeline is intentionally exposed as <c>public</c> so tests can verify it builds correctly.
    /// </summary>
    public static ResiliencePipeline BuildResiliencePipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Constant,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();
    }
}
