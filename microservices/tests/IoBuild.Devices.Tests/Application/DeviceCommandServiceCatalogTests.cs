using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Exceptions;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// Slice 3 (TDD RED-first): DeviceCommandService catalog-picker validation.
///
/// Owner device creation must now validate the typeCode against the global catalog
/// (IDeviceTypeRepository) instead of the OwnerCustomDeviceTypes table.
///
/// Design decisions enforced here:
///   - Catalog type with scope "unit" or "both" → allowed for owner unit devices (200 / 201).
///   - TypeCode NOT in catalog → ArgumentException (controller maps → 400).
///   - TypeCode in catalog but scope is "floor" → ArgumentException with scope-guard message (→ 400).
///   - Owner doesn't own the unit → UnauthorizedAccessException (existing behaviour preserved).
///   - Duplicate same-type device on same unit → DuplicateDeviceTypeOnUnitException (existing → 409).
///
/// Uses SQLite EnsureCreated so the catalog HasData seed rows are available
/// (EnsureCreated applies HasData; no MySQL migrations needed for tests).
/// </summary>
public class DeviceCommandServiceCatalogTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceCommandServiceCatalogTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private DevicesDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseSqlite(_connection)
            .Options;
        var ctx = new DevicesDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private DeviceCommandService BuildService(DevicesDbContext db)
    {
        var deviceRepo = new DeviceRepository(db);
        var outboxRepo = new OutboxMessageRepository(db);
        var typeRepo   = new DeviceTypeRepository(db);
        return new DeviceCommandService(deviceRepo, outboxRepo, db, typeRepo);
    }

    private static void SeedUnitOwnership(DevicesDbContext db, int unitId, int ownerId)
    {
        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = unitId, OwnerUserId = ownerId, UpdatedAt = DateTime.UtcNow
        });
    }

    // ── T1: Owner picks a unit-scoped catalog type → device created (Source=OwnerCustom) ──

    [Fact]
    public async Task CreateOwnerDevice_UnitScopedCatalogType_Succeeds_WithSourceOwnerCustom()
    {
        await using var db = BuildContext();
        SeedUnitOwnership(db, unitId: 50, ownerId: 1);
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        // AirConditioner is a catalog type with scope="unit"
        var device = await svc.Handle(new CreateDeviceCommand(
            "Living Room AC", "AirConditioner", "Unit 50",
            null, ProjectId: 1, "Active",
            UnitId: 50, RequestingOwnerId: "1"));

        device.Should().NotBeNull("catalog type with scope=unit must be accepted");
        device.Source.Should().Be("OwnerCustom",
            "owner-added devices created via catalog picker use the OwnerCustom factory");
    }

    // ── T2: Owner picks a typeCode NOT in the catalog → ArgumentException (→ 400) ──

    [Fact]
    public async Task CreateOwnerDevice_TypeNotInCatalog_ThrowsArgumentException()
    {
        await using var db = BuildContext();
        SeedUnitOwnership(db, unitId: 51, ownerId: 2);
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        // "CO2Sensor" is NOT in the catalog (only in owner_custom_device_types, not device_types)
        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "CO2 Monitor", "CO2Sensor", "Unit 51",
            null, ProjectId: 1, "Active",
            UnitId: 51, RequestingOwnerId: "2"));

        await act.Should().ThrowAsync<ArgumentException>(
            "typeCode not found in catalog must reject with 400-mapped exception");
    }

    // ── T3: Owner picks a floor-scoped catalog type → ArgumentException (scope guard → 400) ──

    [Fact]
    public async Task CreateOwnerDevice_FloorScopedCatalogType_ThrowsArgumentException()
    {
        await using var db = BuildContext();
        SeedUnitOwnership(db, unitId: 52, ownerId: 3);
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        // SmartMeter is a catalog type with scope="floor" — not valid for unit devices
        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "Smart Meter", "SmartMeter", "Unit 52",
            null, ProjectId: 1, "Active",
            UnitId: 52, RequestingOwnerId: "3"));

        await act.Should().ThrowAsync<ArgumentException>(
            "floor-scoped type must be rejected with 400-mapped exception")
            .WithMessage("*cannot be added to a unit*");
    }

    // ── T4: Owner doesn't own the unit → UnauthorizedAccessException (existing behaviour) ──

    [Fact]
    public async Task CreateOwnerDevice_UnitNotOwned_ThrowsUnauthorized()
    {
        await using var db = BuildContext();
        // No UnitOwnerProjection seeded for owner 4 + unit 53
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "AC Unit", "AirConditioner", "Unit 53",
            null, ProjectId: 1, "Active",
            UnitId: 53, RequestingOwnerId: "4"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>(
            "unit not owned by requester must reject with 403-mapped exception")
            .WithMessage("*do not own unit*");
    }

    // ── T5: Duplicate same-type device on same unit → DuplicateDeviceTypeOnUnitException (→ 409) ──

    [Fact]
    public async Task CreateOwnerDevice_DuplicateTypeOnSameUnit_ThrowsDuplicateException()
    {
        await using var db = BuildContext();
        SeedUnitOwnership(db, unitId: 54, ownerId: 5);
        await db.SaveChangesAsync();

        var svc = BuildService(db);

        // First SmartLight succeeds
        await svc.Handle(new CreateDeviceCommand(
            "Bedroom Light", "SmartLight", "Unit 54",
            null, ProjectId: 1, "Active",
            UnitId: 54, RequestingOwnerId: "5"));

        // Second SmartLight on same unit must be rejected
        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "Bedroom Light 2", "SmartLight", "Unit 54",
            null, ProjectId: 1, "Active",
            UnitId: 54, RequestingOwnerId: "5"));

        await act.Should().ThrowAsync<DuplicateDeviceTypeOnUnitException>(
            "duplicate type on same unit must still be rejected (existing 409 behaviour)")
            .WithMessage("*already exists in this unit*");
    }
}
