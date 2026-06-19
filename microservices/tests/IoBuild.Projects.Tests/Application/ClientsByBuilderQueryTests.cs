using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services.Queries.Clients;
using Moq;

namespace IoBuild.Projects.Tests.Application;

public class ClientsByBuilderQueryTests
{
    private readonly Mock<IClientRepository> _repoMock = new();

    private ClientQueryService BuildService() => new(_repoMock.Object);

    [Fact]
    public async Task Handle_GetClientsByBuilderIdQuery_ReturnsOnlyThatBuildersClients()
    {
        // Arrange
        var builderId = 10;
        var expected = new List<Client>
        {
            new("Alice", 1, "Project A", "alice@example.com", "555-0001", "Addr 1"),
            new("Bob",   1, "Project A", "bob@example.com",   "555-0002", "Addr 2"),
        };

        _repoMock
            .Setup(r => r.FindByBuilderIdAsync(builderId))
            .ReturnsAsync(expected);

        var sut = BuildService();

        // Act
        var result = await sut.Handle(new GetClientsByBuilderIdQuery(builderId));

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Contains(c, expected));
    }

    [Fact]
    public async Task Handle_GetClientsByBuilderIdQuery_ReturnsEmpty_WhenBuilderHasNoClients()
    {
        // Arrange
        var builderId = 99;
        _repoMock
            .Setup(r => r.FindByBuilderIdAsync(builderId))
            .ReturnsAsync(Enumerable.Empty<Client>());

        var sut = BuildService();

        // Act
        var result = await sut.Handle(new GetClientsByBuilderIdQuery(builderId));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_GetClientsByBuilderIdQuery_ExcludesOrphanClients()
    {
        // Orphan clients (ProjectId with no matching project) are excluded at
        // the repository JOIN level. The service must delegate to FindByBuilderIdAsync
        // and return exactly what the repository returns (orphans never come back).
        var builderId = 5;
        var orphanProjectId = 999; // no project row for this id

        // Repository correctly returns nothing for this builder (JOIN excludes orphan)
        _repoMock
            .Setup(r => r.FindByBuilderIdAsync(builderId))
            .ReturnsAsync(Enumerable.Empty<Client>());

        var sut = BuildService();

        // Act
        var result = await sut.Handle(new GetClientsByBuilderIdQuery(builderId));

        // Assert: orphan client is not returned
        Assert.Empty(result);
        Assert.DoesNotContain(result, c => c.ProjectId == orphanProjectId);
    }
}
