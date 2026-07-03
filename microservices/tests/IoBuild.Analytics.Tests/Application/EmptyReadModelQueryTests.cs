using FluentAssertions;
using IoBuild.Analytics.Application.Internal.QueryServices;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Infrastructure.InfluxDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IoBuild.Analytics.Tests.Application;

/// <summary>
/// REQ-AQ-03, AQ-S02 — When projection tables are empty, the query service
/// must return zeroed metrics (no exception, no 500, all counts = 0).
/// </summary>
public class EmptyReadModelQueryTests
{
    private static AnalyticsDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AnalyticsDbContext(options);
    }

    /// <summary>
    /// The live-status mock returns no data, so effective status always falls back
    /// to the projection's static Status — preserving these tests' original intent.
    /// </summary>
    private static AnalyticsQueryService CreateService(AnalyticsDbContext db)
    {
        var liveStatus = new Mock<ILiveDeviceStatusService>();
        liveStatus.Setup(s => s.GetLatestStatusesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, string>());

        return new AnalyticsQueryService(
            db, NullLogger<AnalyticsQueryService>.Instance,
            new Mock<ILiveEnergyService>().Object, liveStatus.Object);
    }

    [Fact]
    public async Task GetBuilderDashboard_empty_tables_returns_zeroed_metrics_without_exception()
    {
        // Arrange
        await using var db = BuildContext(nameof(GetBuilderDashboard_empty_tables_returns_zeroed_metrics_without_exception));
        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetBuilderDashboardQuery(UserId: 1));

        // Assert — non-null, all counts zero, no divide-by-zero on OccupancyRate
        result.Should().NotBeNull();
        result!.TotalDevices.Should().Be(0);
        result.OnlineDevices.Should().Be(0);
        result.OfflineDevices.Should().Be(0);
        result.AlertsCount.Should().Be(0);
        result.ActiveProjectsCount.Should().Be(0);
        result.TotalUnits.Should().Be(0);
        result.OccupiedUnits.Should().Be(0);
        result.OccupancyRate.Should().Be(0);
    }

    [Fact]
    public async Task GetOwnerDashboard_empty_tables_returns_zeroed_metrics_without_exception()
    {
        // Arrange
        await using var db = BuildContext(nameof(GetOwnerDashboard_empty_tables_returns_zeroed_metrics_without_exception));
        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetOwnerDashboardQuery(UserId: 2));

        // Assert
        result.Should().NotBeNull();
        result!.TotalDevices.Should().Be(0);
        result.MyUnitsCount.Should().Be(0);
    }

    [Fact]
    public async Task GetBuilderDashboard_computes_builder_device_count_from_projections()
    {
        // Arrange
        await using var db = BuildContext(nameof(GetBuilderDashboard_computes_builder_device_count_from_projections));

        // builder_user_id = 5 owns project 1 and project 2
        db.ProjectProjections.AddRange(
            new IoBuild.Analytics.Domain.Model.Projections.ProjectProjection { ProjectId = 1, BuilderUserId = 5, Name = "Proj A", Status = "OnGoing", LastEventAt = DateTime.UtcNow },
            new IoBuild.Analytics.Domain.Model.Projections.ProjectProjection { ProjectId = 2, BuilderUserId = 5, Name = "Proj B", Status = "OnGoing", LastEventAt = DateTime.UtcNow }
        );
        // 2 devices in project 1, 1 in project 2 → 3 total for builder 5
        db.DeviceProjections.AddRange(
            new IoBuild.Analytics.Domain.Model.Projections.DeviceProjection { DeviceId = 1, ProjectId = 1, OwnerUserId = 0, DeviceType = "Temperature", Status = "Online", LastEventAt = DateTime.UtcNow },
            new IoBuild.Analytics.Domain.Model.Projections.DeviceProjection { DeviceId = 2, ProjectId = 1, OwnerUserId = 0, DeviceType = "Energy", Status = "Offline", LastEventAt = DateTime.UtcNow },
            new IoBuild.Analytics.Domain.Model.Projections.DeviceProjection { DeviceId = 3, ProjectId = 2, OwnerUserId = 0, DeviceType = "Water", Status = "Online", LastEventAt = DateTime.UtcNow },
            // device in another project not owned by builder 5 — must NOT be counted
            new IoBuild.Analytics.Domain.Model.Projections.DeviceProjection { DeviceId = 4, ProjectId = 99, OwnerUserId = 0, DeviceType = "Sensor", Status = "Online", LastEventAt = DateTime.UtcNow }
        );
        // 3 units: 2 for builder 5, 1 for another builder
        db.UnitProjections.AddRange(
            new IoBuild.Analytics.Domain.Model.Projections.UnitProjection { UnitId = 1, ProjectId = 1, BuilderUserId = 5, OwnerUserId = null, Status = "Occupied", LastEventAt = DateTime.UtcNow },
            new IoBuild.Analytics.Domain.Model.Projections.UnitProjection { UnitId = 2, ProjectId = 2, BuilderUserId = 5, OwnerUserId = null, Status = "Vacant", LastEventAt = DateTime.UtcNow },
            new IoBuild.Analytics.Domain.Model.Projections.UnitProjection { UnitId = 3, ProjectId = 99, BuilderUserId = 9, OwnerUserId = null, Status = "Occupied", LastEventAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetBuilderDashboardQuery(UserId: 5));

        // Assert
        result.Should().NotBeNull();
        result!.TotalDevices.Should().Be(3);       // 2 in proj1 + 1 in proj2
        result.OnlineDevices.Should().Be(2);        // devId 1 + 3
        result.OfflineDevices.Should().Be(1);       // devId 2
        result.ActiveProjectsCount.Should().Be(2);  // proj 1 + 2
        result.TotalUnits.Should().Be(2);           // unitId 1 + 2
        result.OccupiedUnits.Should().Be(1);        // unitId 1
        result.OccupancyRate.Should().BeApproximately(50.0, 0.01); // 1/2 * 100
    }
}
