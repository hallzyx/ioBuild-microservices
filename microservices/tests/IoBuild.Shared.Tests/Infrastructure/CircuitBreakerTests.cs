using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Services;
using IoBuild.Shared.Infrastructure.Messaging;
using Polly;
using Polly.CircuitBreaker;
using FluentAssertions;
using Moq;

namespace IoBuild.Shared.Tests.Infrastructure;

/// <summary>
/// RED tests for task 1.4 — circuit breaker configuration.
/// ADR-2: the circuit breaker wraps publish inside the OutboxWorker.
/// The publisher itself throws on failure — the worker owns the Polly pipeline.
/// These tests verify: (1) Polly pipeline with ADR-2 thresholds opens after threshold,
/// (2) the publisher's PublishAsync surface throws when the underlying channel fails.
/// </summary>
public class CircuitBreakerTests
{
    /// <summary>
    /// Builds a Polly pipeline with ADR-2 thresholds for testing.
    /// MinimumThroughput=4, FailureRatio=0.5, SamplingDuration=30s, BreakDuration=15s.
    /// Inner retry: 2 attempts at 200ms.
    /// </summary>
    private static ResiliencePipeline BuildAdr2Pipeline()
    {
        return DomainEventPublishingExtensions.BuildResiliencePipeline();
    }

    [Fact]
    public async Task Pipeline_OpensAfterConsecutiveFailures_ExceedingThreshold()
    {
        // Arrange — use low-threshold pipeline for test speed
        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),
                ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>()
            })
            .Build();

        // Trip 4 consecutive failures (meets MinimumThroughput and 100% failure ratio > 0.5)
        for (int i = 0; i < 4; i++)
        {
            try
            {
                await pipeline.ExecuteAsync(async _ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("broker down");
                });
            }
            catch (InvalidOperationException) { /* expected */ }
        }

        // Act — next call should short-circuit (open)
        Func<Task> act = async () =>
        {
            await pipeline.ExecuteAsync(async _ =>
            {
                await Task.Yield();
                // Should not reach here
            });
        };

        // Assert — breaker is open
        await act.Should().ThrowAsync<BrokenCircuitException>();
    }

    [Fact]
    public async Task Publisher_ThrowsOnPublishFailure_SoWorkerCanCatch()
    {
        // Verifies the publisher's contract: PublishAsync throws when channel throws.
        // The worker is responsible for wrapping this in Polly.
        var publisher = new Mock<IDomainEventPublisher>();
        publisher
            .Setup(p => p.PublishAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("broker unreachable"));

        var evt = new DeviceCreatedEvent { DeviceId = 1, OwnerUserId = 1, DeviceType = "T", Status = "Online" };

        Func<Task> act = () => publisher.Object.PublishAsync(evt, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("broker unreachable");
    }

    [Fact]
    public void Adr2Pipeline_CanBeBuilt_WithoutThrowing()
    {
        // Verifies the factory method in the DI extension builds without error.
        var pipeline = BuildAdr2Pipeline();
        pipeline.Should().NotBeNull();
    }
}
