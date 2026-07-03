using FluentAssertions;
using IoBuild.Analytics.Application.Internal.QueryServices;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Infrastructure.InfluxDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IoBuild.Analytics.Tests.Application;

/// <summary>
/// REQ-2 — Canonical online status taxonomy {online, active} applied at the compare-site.
/// BUG 2: hard-coded "Online" comparison missed "Active" devices created by unit-device path.
/// </summary>
public class AnalyticsQueryServiceTests
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

    // ── T-1.1: Owner Handle must count "Active" device as online ─────────────────

    [Fact]
    public async Task GetDashboardAsync_Owner_CountsActiveDeviceAsOnline()
    {
        // Arrange
        await using var db = BuildContext(nameof(GetDashboardAsync_Owner_CountsActiveDeviceAsOnline));

        // UnitId=10 belongs to owner1 (userId=1)
        db.UnitProjections.Add(new UnitProjection
        {
            UnitId = 10, ProjectId = 1, BuilderUserId = 2,
            OwnerUserId = 1, Status = "Occupied", LastEventAt = DateTime.UtcNow
        });

        // device13 → "Active" (unit-device path sets this, NOT "Online")
        // device16 → "Offline"
        db.DeviceProjections.AddRange(
            new DeviceProjection
            {
                DeviceId = 13, ProjectId = 1, UnitId = 10, OwnerUserId = 0,
                DeviceType = "AirConditioner", Status = "Active", LastEventAt = DateTime.UtcNow
            },
            new DeviceProjection
            {
                DeviceId = 16, ProjectId = 1, UnitId = 10, OwnerUserId = 0,
                DeviceType = "SmartLight", Status = "Offline", LastEventAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetOwnerDashboardQuery(UserId: 1));

        // Assert — "Active" MUST be counted as online (canonical set: {online, active})
        result.Should().NotBeNull();
        result!.OnlineDevices.Should().Be(1, "device13 has Status=\"Active\" which is canonical-online");
        result.OfflineDevices.Should().Be(1, "device16 has Status=\"Offline\"");
    }

    // ── T-1.2: Builder Handle must count "Active" device as online ────────────────

    [Fact]
    public async Task GetDashboardAsync_Builder_CountsActiveDeviceAsOnline()
    {
        // Arrange
        await using var db = BuildContext(nameof(GetDashboardAsync_Builder_CountsActiveDeviceAsOnline));

        // builder1 (userId=3) owns project 5
        db.ProjectProjections.Add(new IoBuild.Analytics.Domain.Model.Projections.ProjectProjection
        {
            ProjectId = 5, BuilderUserId = 3, Name = "Test Project", Status = "OnGoing",
            LastEventAt = DateTime.UtcNow
        });

        // device13 → "Active", device16 → "Offline"
        db.DeviceProjections.AddRange(
            new DeviceProjection
            {
                DeviceId = 13, ProjectId = 5, UnitId = null, OwnerUserId = 0,
                DeviceType = "AirConditioner", Status = "Active", LastEventAt = DateTime.UtcNow
            },
            new DeviceProjection
            {
                DeviceId = 16, ProjectId = 5, UnitId = null, OwnerUserId = 0,
                DeviceType = "SmartLight", Status = "Offline", LastEventAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetBuilderDashboardQuery(UserId: 3));

        // Assert — "Active" MUST be counted as online
        result.Should().NotBeNull();
        result!.OnlineDevices.Should().Be(1, "device13 has Status=\"Active\" which is canonical-online");
        result.OfflineDevices.Should().Be(1, "device16 has Status=\"Offline\"");
    }
}
