using System.Text.Json;
using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Interfaces.Resources;
using IoBuild.Projects.Interfaces.REST;
using IoBuild.Projects.Tests.Infrastructure;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// T-07 [RED] — Per-unit device package tests for ProjectStructureCommandService
/// and ProjectsController (controller-level invalid-room guard).
/// Spec: B1 (per-unit packages), B2 (UnitDevicesDefinedEvent shape).
/// Design ADR-9: units addressed by (floor, roomNumber), NOT unitIndex.
///
/// All tests in this class MUST fail until T-08..T-11 implement the feature.
/// </summary>
public class UnitDevicePackageTests
{
    // ─── T-07-A: valid package → one UnitDevicesDefinedEvent per unit with package ───

    [Fact]
    public async Task Handle_ValidPackage_EmitsOneEventPerUnitWithPackage()
    {
        var db = nameof(Handle_ValidPackage_EmitsOneEventPerUnitWithPackage);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 5);

        var sut = BuildService(ctx);

        // Floor 1, room "01" gets AirConditioner + SmartLight.
        // Floor 1, room "02" has no package (null).
        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec>
                    {
                        new RoomSpec("01", null, new List<string> { "AirConditioner", "SmartLight" }),
                        new RoomSpec("02", null, null)
                    },
                    DeviceTypes: null)
            },
            BuilderId: 5);

        await sut.Handle(command);

        // Assert: exactly one UnitDevicesDefinedEvent (only for room "01")
        var rawMessages = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(UnitDevicesDefinedEvent))
            .ToList();

        rawMessages.Should().HaveCount(1, "only the unit with a non-empty package emits an event");

        var evt = JsonSerializer.Deserialize<UnitDevicesDefinedEvent>(rawMessages[0].Payload)!;
        evt.Floor.Should().Be(1);
        evt.RoomNumber.Should().Be("01");
        evt.ProjectId.Should().Be(project.Id);
        evt.BuilderId.Should().Be(5);
        evt.DeviceTypes.Should().BeEquivalentTo(new[] { "AirConditioner", "SmartLight" });
        evt.UnitId.Should().BeGreaterThan(0, "UnitId must be the real DB id assigned in Phase 1");
    }

    // ─── T-07-B: empty / absent package → no UnitDevicesDefinedEvent emitted ───

    [Fact]
    public async Task Handle_EmptyOrAbsentPackage_EmitsNoUnitDevicesEvent()
    {
        var db = nameof(Handle_EmptyOrAbsentPackage_EmitsNoUnitDevicesEvent);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);

        // No unit has a package
        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec>
                    {
                        new RoomSpec("01", null, null),
                        new RoomSpec("02", null, new List<string>())  // empty list = no package
                    },
                    DeviceTypes: null)
            },
            BuilderId: 1);

        await sut.Handle(command);

        var count = ctx.OutboxMessages
            .Count(m => m.EventType == nameof(UnitDevicesDefinedEvent));

        count.Should().Be(0, "units with no or empty packages must not emit UnitDevicesDefinedEvent");
    }

    // ─── T-07-C: unknown device type in package → rejected before any event ───

    [Fact]
    public async Task Handle_UnknownTypeInPackage_ThrowsArgumentExceptionBeforeEmit()
    {
        var db = nameof(Handle_UnknownTypeInPackage_ThrowsArgumentExceptionBeforeEmit);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);

        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec>
                    {
                        new RoomSpec("01", null, new List<string> { "AirConditioner", "Thermostat" })
                    },
                    DeviceTypes: null)
            },
            BuilderId: 1);

        var act = async () => await sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>(
            "unknown type 'Thermostat' is not in KnownTypes and must be rejected");

        ctx.OutboxMessages.Count(m => m.EventType == nameof(UnitDevicesDefinedEvent))
            .Should().Be(0, "no event must be emitted when validation fails");
    }

    // ─── T-07-D: duplicate types within one unit → rejected ─────────────────

    [Fact]
    public async Task Handle_DuplicateTypesInOneUnit_ThrowsArgumentException()
    {
        var db = nameof(Handle_DuplicateTypesInOneUnit_ThrowsArgumentException);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);

        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec>
                    {
                        new RoomSpec("01", null, new List<string> { "AirConditioner", "AirConditioner" })
                    },
                    DeviceTypes: null)
            },
            BuilderId: 1);

        var act = async () => await sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>(
            "duplicate device types in a single unit package must be rejected");
    }

    // ─── T-07-E: invalid room in UnitDevicePackages resource → HTTP 400 ─────
    // Mirrors the SC-2.4 controller-level guard for out-of-range floor references.

    [Fact]
    public async Task DefineStructure_UnitDevicePackage_InvalidRoom_Returns400()
    {
        var db = nameof(DefineStructure_UnitDevicePackage_InvalidRoom_Returns400);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var controller = BuildController(ctx);

        var resource = new DefineStructureResource
        {
            Floors = 2,
            UnitsPerFloor = 2,  // rooms are "01" and "02" only
            UnitDevicePackages = new List<UnitDevicePackageSelection>
            {
                // "99" is not a valid room for 2 unitsPerFloor
                new UnitDevicePackageSelection { Floor = 1, RoomNumber = "99", DeviceTypes = new List<string> { "AirConditioner" } }
            }
        };

        var result = await controller.DefineStructure(1, resource);

        var badRequest = result as BadRequestObjectResult;
        badRequest.Should().NotBeNull(
            "a UnitDevicePackages entry referencing a non-existent room must return HTTP 400");
        badRequest!.StatusCode.Should().Be(400);
    }

    // ─── T-07-F: valid package carries real UnitId (Phase 1 id) ────────────

    [Fact]
    public async Task Handle_ValidPackage_EventCarriesRealUnitIdFromPhase1()
    {
        var db = nameof(Handle_ValidPackage_EventCarriesRealUnitIdFromPhase1);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);

        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 2,
                    Rooms: new List<RoomSpec>
                    {
                        new RoomSpec("03", null, new List<string> { "SmartLight" })
                    },
                    DeviceTypes: null)
            },
            BuilderId: 1);

        await sut.Handle(command);

        // Verify the event UnitId matches the DB row
        var unit = ctx.Units
            .Single(u => u.ProjectId == project.Id && u.Floor == 2 && u.RoomNumber == "03");

        var rawMessages = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(UnitDevicesDefinedEvent))
            .ToList();

        rawMessages.Should().HaveCount(1);
        var evt = JsonSerializer.Deserialize<UnitDevicesDefinedEvent>(rawMessages[0].Payload)!;
        evt.UnitId.Should().Be(unit.Id, "the event must carry the real Phase-1 assigned UnitId");
    }

    // ─── T-07-G: floor provisioning unaffected when unit packages present ────

    [Fact]
    public async Task Handle_WithUnitPackage_FloorEventsStillEmitted()
    {
        var db = nameof(Handle_WithUnitPackage_FloorEventsStillEmitted);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);

        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec>
                    {
                        new RoomSpec("01", null, new List<string> { "AirConditioner" })
                    },
                    DeviceTypes: new List<string> { "SmartMeter" })
            },
            BuilderId: 1);

        await sut.Handle(command);

        ctx.OutboxMessages.Count(m => m.EventType == nameof(FloorStructureDefinedEvent))
            .Should().Be(1, "floor events must still be emitted when unit packages are present");

        ctx.OutboxMessages.Count(m => m.EventType == nameof(UnitDevicesDefinedEvent))
            .Should().Be(1, "unit package event must also be emitted");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static ProjectStructureCommandService BuildService(
        IoBuild.Projects.Infrastructure.Persistence.AppDbContext ctx)
    {
        var unitRepo = new UnitRepository(ctx);
        var projectRepo = new ProjectRepository(ctx);
        var outboxRepo = new OutboxMessageRepository(ctx);
        return new ProjectStructureCommandService(unitRepo, projectRepo, outboxRepo, ctx);
    }

    private static ProjectsController BuildController(
        IoBuild.Projects.Infrastructure.Persistence.AppDbContext ctx)
    {
        var structureService = new ProjectStructureCommandService(
            new UnitRepository(ctx),
            new ProjectRepository(ctx),
            new OutboxMessageRepository(ctx),
            ctx);

        var cmdService = new Mock<IoBuild.Projects.Domain.Services.IProjectCommandService>().Object;
        var qryService = new Mock<IoBuild.Projects.Domain.Services.IProjectQueryService>().Object;

        var controller = new ProjectsController(cmdService, qryService, structureService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Items["UserRole"] = "Builder";
        controller.HttpContext.Items["UserId"] = 1;
        return controller;
    }
}
