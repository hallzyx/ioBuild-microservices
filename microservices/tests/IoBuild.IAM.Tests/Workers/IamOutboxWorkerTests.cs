using FluentAssertions;
using IoBuild.IAM.Domain.Model.Entities;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.IAM.Workers;
using IoBuild.Shared.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace IoBuild.IAM.Tests.Workers;

/// <summary>
/// RED tests for task 4.2 — IAM OutboxWorker publish behavior.
/// Mirrors IoBuild.Devices.Tests.Workers.OutboxWorkerPublishTests exactly.
/// </summary>
public class IamOutboxWorkerTests
{
    private static OutboxWorker BuildWorker(
        Mock<IIamOutboxMessageRepository> outboxRepo,
        Mock<IDomainEventPublisher> publisher,
        Mock<ILogger<OutboxWorker>> logger)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => outboxRepo.Object);

        var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        return new OutboxWorker(scopeFactory, publisher.Object, logger.Object);
    }

    [Fact]
    public async Task PublishSuccess_MarksRowProcessed()
    {
        // Arrange
        var outboxRepo = new Mock<IIamOutboxMessageRepository>();
        var publisher = new Mock<IDomainEventPublisher>();
        var logger = new Mock<ILogger<OutboxWorker>>();

        var pendingMsg = new OutboxMessage("UserRegisteredEvent",
            """{"UserId":1,"Email":"owner@test.com","Role":"owner","EventId":"00000000-0000-0000-0000-000000000001","OccurredOn":"2024-01-01T00:00:00Z","RoutingKey":"iam.user.registered"}""")
        {
            EventId = Guid.NewGuid()
        };

        outboxRepo.Setup(r => r.GetPendingAsync())
            .ReturnsAsync(new List<OutboxMessage> { pendingMsg });
        outboxRepo.Setup(r => r.UpdateAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);

        publisher.Setup(p => p.PublishAsync(
                It.IsAny<IoBuild.Shared.Domain.Model.Events.DomainEvent>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var worker = BuildWorker(outboxRepo, publisher, logger);

        // Act
        await worker.RunOneCycleAsync(CancellationToken.None);

        // Assert
        pendingMsg.Status.Should().Be("Processed");
        pendingMsg.ProcessedAt.Should().NotBeNull();
        pendingMsg.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishFailure_IncrementsRetryCount_DoesNotThrow()
    {
        // Arrange
        var outboxRepo = new Mock<IIamOutboxMessageRepository>();
        var publisher = new Mock<IDomainEventPublisher>();
        var logger = new Mock<ILogger<OutboxWorker>>();

        var pendingMsg = new OutboxMessage("UserRegisteredEvent",
            """{"UserId":1,"Email":"owner@test.com","Role":"owner","EventId":"00000000-0000-0000-0000-000000000001","OccurredOn":"2024-01-01T00:00:00Z","RoutingKey":"iam.user.registered"}""")
        {
            EventId = Guid.NewGuid()
        };

        outboxRepo.Setup(r => r.GetPendingAsync())
            .ReturnsAsync(new List<OutboxMessage> { pendingMsg });
        outboxRepo.Setup(r => r.UpdateAsync(It.IsAny<OutboxMessage>())).Returns(Task.CompletedTask);

        publisher.Setup(p => p.PublishAsync(
                It.IsAny<IoBuild.Shared.Domain.Model.Events.DomainEvent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("broker unreachable"));

        var worker = BuildWorker(outboxRepo, publisher, logger);

        // Act
        Func<Task> act = async () => await worker.RunOneCycleAsync(CancellationToken.None);

        // Assert — must not throw
        await act.Should().NotThrowAsync();
        pendingMsg.Status.Should().Be("Pending");
        pendingMsg.RetryCount.Should().Be(1);
        pendingMsg.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task NoPendingMessages_DoesNotCallPublisher()
    {
        // Arrange
        var outboxRepo = new Mock<IIamOutboxMessageRepository>();
        var publisher = new Mock<IDomainEventPublisher>();
        var logger = new Mock<ILogger<OutboxWorker>>();

        outboxRepo.Setup(r => r.GetPendingAsync())
            .ReturnsAsync(new List<OutboxMessage>());

        var worker = BuildWorker(outboxRepo, publisher, logger);

        // Act
        await worker.RunOneCycleAsync(CancellationToken.None);

        // Assert
        publisher.Verify(p => p.PublishAsync(
            It.IsAny<IoBuild.Shared.Domain.Model.Events.DomainEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
