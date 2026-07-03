using FluentAssertions;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Commands;
using IoBuild.Devices.Domain.Model.Exceptions;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Shared.Domain.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// D-3 (TDD RED-first): 409 duplicate-type-on-unit detection in DeviceCommandService.
///
/// The service pre-checks for existing OwnerCustom devices with the same (project, unit, type)
/// before inserting, and throws DuplicateDeviceTypeOnUnitException → controller maps to 409.
///
/// Uses SQLite so that IF the unique index also fires (belt-and-suspenders), it is also caught.
/// The pre-check means the test also works on InMemory (no unique-index dependency for the 409 path).
/// </summary>
public class DeviceCommandService409Tests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceCommandService409Tests()
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

    private DeviceCommandService BuildService(DevicesDbContext db) =>
        new(new DeviceRepository(db), new OutboxMessageRepository(db), db, new DeviceTypeRepository(db));

    // ── D-3a: Same catalog type on same unit → DuplicateDeviceTypeOnUnitException ──

    [Fact]
    public async Task CreateOwnerCustomDevice_DuplicateTypeOnSameUnit_ThrowsDuplicateException()
    {
        await using var db = BuildContext();

        // Seed: owner projection so service can validate ownership
        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 10, OwnerUserId = 1, UpdatedAt = DateTime.UtcNow
        });

        // Seed: catalog type so the service accepts this typeCode
        db.DeviceTypes.Add(new DeviceType(
            "CO2Sensor", "CO2 Sensor", "unit",
            [new DeviceCapabilityCatalog.ControllableAttribute("co2", "number", 0, 5000, "ppm")]));

        await db.SaveChangesAsync();

        var svc = BuildService(db);

        var firstCommand = new CreateDeviceCommand(
            "Sensor 1", "CO2Sensor", "Unit 01",
            "OC:AA:BB:CC:01:01", 1, "Active",
            UnitId: 10, RequestingOwnerId: "1");

        await svc.Handle(firstCommand);   // first one succeeds

        // Second device — same type, same unit, different MAC
        var secondCommand = new CreateDeviceCommand(
            "Sensor 2", "CO2Sensor", "Unit 01",
            "OC:AA:BB:CC:01:02", 1, "Active",
            UnitId: 10, RequestingOwnerId: "1");

        var act = async () => await svc.Handle(secondCommand);

        await act.Should().ThrowAsync<DuplicateDeviceTypeOnUnitException>()
            .WithMessage("*already exists in this unit*");
    }

    // ── D-3c: UnitId not owned by requesting owner → UnauthorizedAccessException ──
    // Guard lives in DeviceCommandService ~line 44–53. No UnitOwnerProjection row exists
    // for this (ownerId, unitId) pair, so the ownership check fails before any insert.

    [Fact]
    public async Task CreateOwnerCustomDevice_UnitNotOwnedByRequester_ThrowsUnauthorized()
    {
        await using var db = BuildContext();

        // Deliberately DO NOT seed a UnitOwnerProjection for owner "1" + unit 99.
        // Owner "2" owns unit 99 — owner "1" must be rejected.
        db.UnitOwnerProjections.Add(new UnitOwnerProjection
        {
            UnitId = 99, OwnerUserId = 2, UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var svc = BuildService(db);

        var command = new CreateDeviceCommand(
            "Intruder Sensor", "CO2Sensor", "Someone Else's Unit",
            "OC:DD:EE:FF:00:01", 1, "Active",
            UnitId: 99, RequestingOwnerId: "1");   // owner "1" does NOT own unit 99

        var act = async () => await svc.Handle(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>(
            "unit 99 belongs to owner 2, not owner 1 — ownership guard must fire")
            .WithMessage("*do not own unit*");
    }

    // ── D-3b: Same type on different unit → 201 (no conflict) ────────────────

    [Fact]
    public async Task CreateOwnerCustomDevice_SameTypeOnDifferentUnit_Succeeds()
    {
        await using var db = BuildContext();

        // Seed two unit projections for owner 1
        db.UnitOwnerProjections.AddRange(
            new UnitOwnerProjection { UnitId = 11, OwnerUserId = 1, UpdatedAt = DateTime.UtcNow },
            new UnitOwnerProjection { UnitId = 12, OwnerUserId = 1, UpdatedAt = DateTime.UtcNow });

        db.DeviceTypes.Add(new DeviceType(
            "CO2Sensor2", "CO2 Sensor", "unit",
            [new DeviceCapabilityCatalog.ControllableAttribute("co2", "number", 0, 5000, "ppm")]));

        await db.SaveChangesAsync();

        var svc = BuildService(db);

        await svc.Handle(new CreateDeviceCommand(
            "Sensor A", "CO2Sensor2", "Unit 01",
            "OC:AA:BB:CC:02:01", 1, "Active", UnitId: 11, RequestingOwnerId: "1"));

        var act = async () => await svc.Handle(new CreateDeviceCommand(
            "Sensor B", "CO2Sensor2", "Unit 02",
            "OC:AA:BB:CC:02:02", 1, "Active", UnitId: 12, RequestingOwnerId: "1"));


        await act.Should().NotThrowAsync("same type on different unit is allowed");
    }
}
