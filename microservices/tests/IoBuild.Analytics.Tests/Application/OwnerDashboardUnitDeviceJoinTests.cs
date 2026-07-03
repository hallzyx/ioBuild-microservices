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
/// T-18 / REQ-D1 — AnalyticsQueryService.Handle(GetOwnerDashboardQuery) must derive
/// the requesting owner's devices via a read-side join:
///   DeviceProjection.UnitId ↔ UnitProjection.UnitId WHERE UnitProjection.OwnerUserId == requestingUser
///
/// It must NOT rely on DeviceProjection.OwnerUserId (always 0).
/// Uses correlated EXISTS / Any() — never List.Contains — so queries are MySQL-translatable.
/// Telemetry values (EnergyThisMonth, TemperatureAvg) are out of scope and NOT asserted.
/// </summary>
public class OwnerDashboardUnitDeviceJoinTests
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

    // ── Scenario 1: owner WITH a matched unit sees their unit's devices ──────────

    [Fact]
    public async Task Owner_with_matched_unit_sees_their_unit_devices_count()
    {
        // Arrange
        await using var db = BuildContext(nameof(Owner_with_matched_unit_sees_their_unit_devices_count));

        // UnitId=42 belongs to owner 7
        db.UnitProjections.Add(new UnitProjection
        {
            UnitId = 42, ProjectId = 1, BuilderUserId = 3,
            OwnerUserId = 7, Status = "Occupied", LastEventAt = DateTime.UtcNow
        });
        // Two devices provisioned under UnitId=42
        db.DeviceProjections.AddRange(
            new DeviceProjection { DeviceId = 10, ProjectId = 1, UnitId = 42, OwnerUserId = 0, DeviceType = "AirConditioner", Status = "Online",  LastEventAt = DateTime.UtcNow },
            new DeviceProjection { DeviceId = 11, ProjectId = 1, UnitId = 42, OwnerUserId = 0, DeviceType = "SmartLight",     Status = "Offline", LastEventAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetOwnerDashboardQuery(UserId: 7));

        // Assert
        result.Should().NotBeNull();
        result!.TotalDevices.Should().Be(2, "owner 7 has two unit-package devices via the UnitId join");
        result.TotalDevices.Should().NotBe(0, "must NOT be zero when devices exist for the owner's unit");
    }

    [Fact]
    public async Task Owner_with_matched_unit_sees_correct_device_types_in_health_status()
    {
        // Arrange
        await using var db = BuildContext(nameof(Owner_with_matched_unit_sees_correct_device_types_in_health_status));

        db.UnitProjections.Add(new UnitProjection
        {
            UnitId = 42, ProjectId = 1, BuilderUserId = 3,
            OwnerUserId = 7, Status = "Occupied", LastEventAt = DateTime.UtcNow
        });
        db.DeviceProjections.AddRange(
            new DeviceProjection { DeviceId = 10, ProjectId = 1, UnitId = 42, OwnerUserId = 0, DeviceType = "AirConditioner", Status = "Online",  LastEventAt = DateTime.UtcNow },
            new DeviceProjection { DeviceId = 11, ProjectId = 1, UnitId = 42, OwnerUserId = 0, DeviceType = "SmartLight",     Status = "Offline", LastEventAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetOwnerDashboardQuery(UserId: 7));

        // Assert
        result.Should().NotBeNull();
        result!.DeviceHealthStatus.Should().HaveCount(2);
        result.DeviceHealthStatus.Select(d => d.Status)
            .Should().BeEquivalentTo(new[] { "Online", "Offline" });
    }

    // ── Scenario 2: owner with NO matched unit sees zero devices ─────────────────

    [Fact]
    public async Task Owner_with_no_matched_unit_sees_zero_devices()
    {
        // Arrange
        await using var db = BuildContext(nameof(Owner_with_no_matched_unit_sees_zero_devices));

        // No UnitProjection with OwnerUserId=8
        db.DeviceProjections.Add(
            new DeviceProjection { DeviceId = 20, ProjectId = 1, UnitId = 99, OwnerUserId = 0, DeviceType = "AirConditioner", Status = "Online", LastEventAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act
        var result = await svc.Handle(new GetOwnerDashboardQuery(UserId: 8));

        // Assert
        result.Should().NotBeNull();
        result!.TotalDevices.Should().Be(0, "owner 8 has no matched unit — must see zero devices");
        result.DeviceHealthStatus.Should().BeEmpty();
    }

    // ── Scenario 3: owner does NOT see another owner's devices ───────────────────

    [Fact]
    public async Task Owner_A_does_not_see_owner_B_unit_devices()
    {
        // Arrange
        await using var db = BuildContext(nameof(Owner_A_does_not_see_owner_B_unit_devices));

        // Owner A (userId=7) owns UnitId=42; Owner B (userId=9) owns UnitId=55
        db.UnitProjections.AddRange(
            new UnitProjection { UnitId = 42, ProjectId = 1, BuilderUserId = 3, OwnerUserId = 7, Status = "Occupied", LastEventAt = DateTime.UtcNow },
            new UnitProjection { UnitId = 55, ProjectId = 1, BuilderUserId = 3, OwnerUserId = 9, Status = "Occupied", LastEventAt = DateTime.UtcNow }
        );
        // Owner A's devices
        db.DeviceProjections.AddRange(
            new DeviceProjection { DeviceId = 10, ProjectId = 1, UnitId = 42, OwnerUserId = 0, DeviceType = "AirConditioner", Status = "Online", LastEventAt = DateTime.UtcNow },
            new DeviceProjection { DeviceId = 11, ProjectId = 1, UnitId = 42, OwnerUserId = 0, DeviceType = "SmartLight",     Status = "Online", LastEventAt = DateTime.UtcNow }
        );
        // Owner B's device on a different unit
        db.DeviceProjections.Add(
            new DeviceProjection { DeviceId = 20, ProjectId = 1, UnitId = 55, OwnerUserId = 0, DeviceType = "SmartMeter", Status = "Online", LastEventAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act — query as Owner A
        var result = await svc.Handle(new GetOwnerDashboardQuery(UserId: 7));

        // Assert — Owner A sees exactly 2, never DeviceId=20 (Owner B's unit)
        result.Should().NotBeNull();
        result!.TotalDevices.Should().Be(2, "owner A must only see their own unit's devices");
        result.DeviceHealthStatus.Select(d => d.DeviceId).Should().NotContain(20, "device 20 belongs to owner B's unit");
    }

    // ── Scenario 4: device exists but unit NOT yet owner-matched → invisible ─────

    [Fact]
    public async Task Device_with_unmatched_unit_is_not_visible_to_owner()
    {
        // Arrange
        await using var db = BuildContext(nameof(Device_with_unmatched_unit_is_not_visible_to_owner));

        // UnitId=42 has OwnerUserId=0 (not yet matched by UnitOwnerMatchedEvent)
        db.UnitProjections.Add(new UnitProjection
        {
            UnitId = 42, ProjectId = 1, BuilderUserId = 3,
            OwnerUserId = 0, Status = "Vacant", LastEventAt = DateTime.UtcNow
        });
        db.DeviceProjections.Add(
            new DeviceProjection { DeviceId = 10, ProjectId = 1, UnitId = 42, OwnerUserId = 0, DeviceType = "AirConditioner", Status = "Online", LastEventAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var svc = CreateService(db);

        // Act — query as the eventual owner (userId=7) BEFORE the match event arrives
        var result = await svc.Handle(new GetOwnerDashboardQuery(UserId: 7));

        // Assert — device is invisible until UnitOwnerMatchedEvent updates OwnerUserId
        result.Should().NotBeNull();
        result!.TotalDevices.Should().Be(0, "device is invisible until unit is owner-matched");
    }
}
