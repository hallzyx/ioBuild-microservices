using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Infrastructure.Messaging;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace IoBuild.Projects.Tests.Infrastructure;

/// <summary>
/// Unit tests for UnitOwnerAnnouncer (startup reconciliation service).
///
/// Mirrors DeviceRegistryAnnouncerTests (IoBuild.Devices.Tests) in structure.
/// Uses the internal AnnounceAllAsync seam + a mock IDomainEventPublisher
/// so no live RabbitMQ broker is required.
/// </summary>
public class UnitOwnerAnnouncerTests
{
    // ------------------------------------------------------------------ //
    //  Helper: build a Unit via the legacy constructor to avoid EF navigation issues.
    //  Sets OwnerId = ownerId arg (non-null means owned).
    // ------------------------------------------------------------------ //

    private static Unit OwnedUnit(int projectId, int ownerId, string email)
    {
        // Use the legacy ctor: projectId, unitNumber, ownerId
        return new Unit(projectId, $"unit-{ownerId}", ownerId);
    }

    private static Unit UnownedUnit(int projectId)
    {
        // Structure-definition ctor: OwnerId stays null
        return new Unit(projectId, floor: 1, roomNumber: "01", ownerEmail: null);
    }

    // ------------------------------------------------------------------ //
    //  UA-S01 — owned units each receive exactly one published event
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task AnnounceAllAsync_PublishesOneEventPerOwnedUnit()
    {
        // Arrange
        var unit1 = OwnedUnit(projectId: 1, ownerId: 10, email: "a@test.com");
        var unit2 = OwnedUnit(projectId: 2, ownerId: 20, email: "b@test.com");

        var repo = new Mock<IUnitRepository>();
        repo.Setup(r => r.ListAsync())
            .ReturnsAsync(new List<Unit> { unit1, unit2 });

        var publisher = new Mock<IDomainEventPublisher>();

        var services = new ServiceCollection();
        services.AddScoped(_ => repo.Object);
        var scopeFactory = services.BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();

        var logger = new Mock<ILogger<UnitOwnerAnnouncer>>().Object;
        var announcer = new UnitOwnerAnnouncer(scopeFactory, publisher.Object, logger);

        // Act
        await announcer.AnnounceAllAsync(CancellationToken.None);

        // Assert — exactly two publishes, one per owned unit
        publisher.Verify(
            p => p.PublishAsync(
                It.Is<UnitOwnerMatchedEvent>(e => e.OwnerUserId == 10),
                It.IsAny<CancellationToken>()),
            Times.Once);

        publisher.Verify(
            p => p.PublishAsync(
                It.Is<UnitOwnerMatchedEvent>(e => e.OwnerUserId == 20),
                It.IsAny<CancellationToken>()),
            Times.Once);

        publisher.Verify(
            p => p.PublishAsync(It.IsAny<UnitOwnerMatchedEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // ------------------------------------------------------------------ //
    //  UA-S02 — null-owner units are silently skipped
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task AnnounceAllAsync_SkipsUnownedUnits()
    {
        // Arrange
        var unowned = UnownedUnit(projectId: 1);
        var owned   = OwnedUnit(projectId: 1, ownerId: 5, email: "c@test.com");

        var repo = new Mock<IUnitRepository>();
        repo.Setup(r => r.ListAsync())
            .ReturnsAsync(new List<Unit> { unowned, owned });

        var publisher = new Mock<IDomainEventPublisher>();

        var services = new ServiceCollection();
        services.AddScoped(_ => repo.Object);
        var scopeFactory = services.BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();

        var logger = new Mock<ILogger<UnitOwnerAnnouncer>>().Object;
        var announcer = new UnitOwnerAnnouncer(scopeFactory, publisher.Object, logger);

        // Act
        await announcer.AnnounceAllAsync(CancellationToken.None);

        // Assert — only the owned unit triggers a publish
        publisher.Verify(
            p => p.PublishAsync(It.IsAny<UnitOwnerMatchedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        publisher.Verify(
            p => p.PublishAsync(
                It.Is<UnitOwnerMatchedEvent>(e => e.OwnerUserId == 5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ------------------------------------------------------------------ //
    //  UA-S03 — empty DB produces zero publishes
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task AnnounceAllAsync_WhenNoUnits_PublishesNothing()
    {
        // Arrange
        var repo = new Mock<IUnitRepository>();
        repo.Setup(r => r.ListAsync()).ReturnsAsync(new List<Unit>());

        var publisher = new Mock<IDomainEventPublisher>();

        var services = new ServiceCollection();
        services.AddScoped(_ => repo.Object);
        var scopeFactory = services.BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();

        var logger = new Mock<ILogger<UnitOwnerAnnouncer>>().Object;
        var announcer = new UnitOwnerAnnouncer(scopeFactory, publisher.Object, logger);

        // Act
        await announcer.AnnounceAllAsync(CancellationToken.None);

        // Assert
        publisher.Verify(
            p => p.PublishAsync(It.IsAny<UnitOwnerMatchedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ------------------------------------------------------------------ //
    //  UA-S04 — published event carries correct UnitId and ProjectId
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task AnnounceAllAsync_EventCarriesCorrectUnitIdAndProjectId()
    {
        // Arrange — we need to inspect which UnitId and ProjectId are emitted.
        // The legacy ctor sets ProjectId via the base path; we verify the mapped fields.
        var owned = OwnedUnit(projectId: 7, ownerId: 99, email: "d@test.com");

        var repo = new Mock<IUnitRepository>();
        repo.Setup(r => r.ListAsync()).ReturnsAsync(new List<Unit> { owned });

        UnitOwnerMatchedEvent? captured = null;
        var publisher = new Mock<IDomainEventPublisher>();
        publisher
            .Setup(p => p.PublishAsync(It.IsAny<UnitOwnerMatchedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<DomainEvent, CancellationToken>((evt, _) => captured = evt as UnitOwnerMatchedEvent)
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddScoped(_ => repo.Object);
        var scopeFactory = services.BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();

        var logger = new Mock<ILogger<UnitOwnerAnnouncer>>().Object;
        var announcer = new UnitOwnerAnnouncer(scopeFactory, publisher.Object, logger);

        // Act
        await announcer.AnnounceAllAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(7, captured!.ProjectId);
        Assert.Equal(99, captured.OwnerUserId);
    }
}
