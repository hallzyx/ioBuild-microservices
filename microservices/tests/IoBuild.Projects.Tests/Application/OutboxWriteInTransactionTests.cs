using System.Text.Json;
using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Domain.Services.Commands.Units;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Tests.Infrastructure;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Repositories;
using Moq;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// TDD RED tests: assert that ProjectCommandService and UnitCommandService write exactly one
/// OutboxMessage row in the same transaction as the aggregate change (REQ-DE-02, DE-S07).
/// </summary>
public class OutboxWriteInTransactionTests
{
    [Fact]
    public async Task Handle_CreateProject_AddsOutboxRowBeforeCompleteAsync()
    {
        // Arrange
        var projectRepo = new Mock<IProjectRepository>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var addedMessages = new List<OutboxMessage>();
        outboxRepo
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => addedMessages.Add(m))
            .Returns(Task.CompletedTask);

        var sut = new ProjectCommandService(projectRepo.Object, unitOfWork.Object, outboxRepo.Object);
        var command = new CreateProjectCommand("Test Project", "Desc", "Lima", 10, 42, "http://img.png");

        // Act
        await sut.Handle(command);

        // Assert: exactly one outbox row was added
        Assert.Single(addedMessages);
        Assert.Equal("ProjectCreatedEvent", addedMessages[0].EventType);
        Assert.NotEqual(Guid.Empty, addedMessages[0].EventId);

        // CompleteAsync was called exactly once (single transaction)
        unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateProject_AddsOutboxRowBeforeCompleteAsync()
    {
        // Arrange
        var project = new Project("Name", "Desc", "Lima", 10, 1, "http://img.png");
        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(project);

        var outboxRepo = new Mock<IOutboxMessageRepository>();
        var addedMessages = new List<OutboxMessage>();
        outboxRepo
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => addedMessages.Add(m))
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();

        var sut = new ProjectCommandService(projectRepo.Object, unitOfWork.Object, outboxRepo.Object);
        var command = new UpdateProjectCommand(1, "New Name", "New Desc", "Callao", 20, 5,
            IoBuild.Projects.Domain.Model.ValueObjects.EProjectStatus.OnGoing, "http://img2.png");

        // Act
        await sut.Handle(command);

        // Assert
        Assert.Single(addedMessages);
        Assert.Equal("ProjectUpdatedEvent", addedMessages[0].EventType);
        unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Task 1.3 [RED] — Two-phase commit assertion (Moq).
    /// After the ADR-A fix, UnitCommandService must call CompleteAsync TWICE:
    ///   (1) persist the unit so unit.Id is real,
    ///   (2) persist the outbox row with the real UnitId.
    /// This assertion FAILS on the current single-CompleteAsync implementation.
    /// </summary>
    [Fact]
    public async Task Handle_CreateUnit_CallsCompleteAsyncTwice_TwoPhaseCommit()
    {
        // Arrange
        var unitRepo = new Mock<IUnitRepository>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();
        var addedMessages = new List<OutboxMessage>();
        outboxRepo
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => addedMessages.Add(m))
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        var projectRepo = new Mock<IProjectRepository>();

        var sut = new UnitCommandService(unitRepo.Object, unitOfWork.Object, outboxRepo.Object, projectRepo.Object);
        var command = new CreateUnitCommand(5, "101-A", 99);

        // Act
        await sut.Handle(command);

        // Assert — ADR-A: two-phase commit; first CompleteAsync persists the unit (real Id),
        // second CompleteAsync persists the outbox row with the real UnitId.
        Assert.Single(addedMessages);
        Assert.Equal("UnitCreatedEvent", addedMessages[0].EventType);
        Assert.NotEqual(Guid.Empty, addedMessages[0].EventId);
        // RED: current code calls CompleteAsync once → this assertion fails until Task 1.4 fix.
        unitOfWork.Verify(u => u.CompleteAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_CreateUnit_AddsOutboxRowBeforeCompleteAsync()
    {
        // Arrange
        var unitRepo = new Mock<IUnitRepository>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();
        var addedMessages = new List<OutboxMessage>();
        outboxRepo
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => addedMessages.Add(m))
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        var projectRepo = new Mock<IProjectRepository>();

        var sut = new UnitCommandService(unitRepo.Object, unitOfWork.Object, outboxRepo.Object, projectRepo.Object);
        var command = new CreateUnitCommand(5, "101-A", 99);

        // Act
        await sut.Handle(command);

        // Assert — kept for backward compat; validates outbox row is written
        Assert.Single(addedMessages);
        Assert.Equal("UnitCreatedEvent", addedMessages[0].EventType);
        Assert.NotEqual(Guid.Empty, addedMessages[0].EventId);
        // NOTE: After Task 1.4 this verifies Times.Exactly(2) — see Handle_CreateUnit_CallsCompleteAsyncTwice_TwoPhaseCommit
        unitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce());
    }

    /// <summary>
    /// Task 1.3 [RED] — Uses real EF InMemory so unit.Id is assigned by the provider
    /// on SaveChanges. Current code builds UnitCreatedEvent BEFORE CompleteAsync, so
    /// unit.Id is 0 at that point → this test MUST FAIL on the current implementation.
    /// After the Task 1.4 fix (two-phase commit) this test becomes GREEN (Task 1.5).
    /// </summary>
    [Fact]
    public async Task Handle_CreateUnit_WritesOutboxWithRealId()
    {
        // Arrange — real EF InMemory context wired through concrete repositories
        var dbName = nameof(Handle_CreateUnit_WritesOutboxWithRealId);
        await using var ctx = ProjectsDbFixture.NewContext(dbName);

        // Seed a parent project so FindByIdAsync returns a real BuilderId
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 7);

        var unitRepo = new UnitRepository(ctx);
        var outboxRepo = new OutboxMessageRepository(ctx);
        // AppDbContext implements IUnitOfWork (calls SaveChangesAsync)
        var sut = new UnitCommandService(unitRepo, ctx, outboxRepo, new ProjectRepository(ctx));

        var command = new CreateUnitCommand(project.Id, "3-01", 0);

        // Act
        await sut.Handle(command);

        // Assert — read back the outbox row and deserialise the payload
        var outboxRow = ctx.OutboxMessages.Single(m => m.EventType == nameof(UnitCreatedEvent));
        var evt = JsonSerializer.Deserialize<UnitCreatedEvent>(outboxRow.Payload);

        evt.Should().NotBeNull();
        evt!.UnitId.Should().BeGreaterThan(0,
            "UnitCreatedEvent.UnitId must carry the real DB-assigned identity, " +
            "not the pre-commit value of 0 (ADR-A / Task 1.4 fix)");
    }
}
