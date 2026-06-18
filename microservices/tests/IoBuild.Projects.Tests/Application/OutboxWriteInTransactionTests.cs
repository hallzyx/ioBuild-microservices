using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Domain.Services.Commands.Units;
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

        // Assert
        Assert.Single(addedMessages);
        Assert.Equal("UnitCreatedEvent", addedMessages[0].EventType);
        Assert.NotEqual(Guid.Empty, addedMessages[0].EventId);
        unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
