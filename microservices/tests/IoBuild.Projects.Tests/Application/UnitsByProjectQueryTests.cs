using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services.Queries.Units;
using Moq;

namespace IoBuild.Projects.Tests.Application;

public class UnitsByProjectQueryTests
{
    private readonly Mock<IUnitRepository> _repositoryMock;
    private readonly UnitQueryService _service;

    public UnitsByProjectQueryTests()
    {
        _repositoryMock = new Mock<IUnitRepository>();
        _service = new UnitQueryService(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_GetUnitsByProjectIdQuery_ReturnsOnlyUnitsForThatProject()
    {
        // Arrange
        var projectId = 42;
        var expectedUnits = new List<Unit>
        {
            new Unit(projectId, "A-101", 1),
            new Unit(projectId, "A-102", 2)
        };

        _repositoryMock
            .Setup(r => r.FindByProjectIdAsync(projectId))
            .ReturnsAsync(expectedUnits);

        var query = new GetUnitsByProjectIdQuery(projectId);

        // Act
        var result = await _service.Handle(query);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.ProjectId == projectId);
        _repositoryMock.Verify(r => r.FindByProjectIdAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task Handle_GetUnitsByProjectIdQuery_ReturnsEmptyList_WhenProjectHasNoUnits()
    {
        // Arrange
        var projectId = 99;

        _repositoryMock
            .Setup(r => r.FindByProjectIdAsync(projectId))
            .ReturnsAsync(new List<Unit>());

        var query = new GetUnitsByProjectIdQuery(projectId);

        // Act
        var result = await _service.Handle(query);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.FindByProjectIdAsync(projectId), Times.Once);
    }
}
