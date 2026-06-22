using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Interfaces.Resources;
using IoBuild.Projects.Interfaces.REST;
using IoBuild.Projects.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// SC-2.4: Out-of-range floor reference in floorDeviceTypes must return HTTP 400.
/// The guard fires in ProjectsController BEFORE the service is called,
/// so a real (but idle) service is fine here — the guard is the unit under test.
/// </summary>
public class DefineStructureControllerValidationTests
{
    // ── SC-2.4: floor=5 when floors=2 → HTTP 400 ────────────────────────────

    [Fact]
    public async Task DefineStructure_OutOfRangeFloor_Returns400()
    {
        // Arrange — real service backed by in-memory DB (guard fires before service is called)
        var db = nameof(DefineStructure_OutOfRangeFloor_Returns400);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var structureService = new ProjectStructureCommandService(
            new UnitRepository(ctx),
            new ProjectRepository(ctx),
            new OutboxMessageRepository(ctx),
            ctx);

        var controller = BuildController(structureService);

        var resource = new DefineStructureResource
        {
            Floors = 2,
            UnitsPerFloor = 1,
            DeviceTypesPerFloor = new List<FloorDeviceTypeSelection>
            {
                // Floor 5 is out of range for a 2-floor building
                new FloorDeviceTypeSelection { Floor = 5, DeviceTypes = new List<string> { "SmartMeter" } }
            }
        };

        // Act
        var result = await controller.DefineStructure(1, resource);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        badRequest.Should().NotBeNull(
            "SC-2.4: an out-of-range floor reference in floorDeviceTypes must return HTTP 400, not be silently ignored");
        badRequest!.StatusCode.Should().Be(400);
    }

    // ── Triangulation: floor=0 is also out of range → HTTP 400 ──────────────

    [Fact]
    public async Task DefineStructure_FloorZero_Returns400()
    {
        var db = nameof(DefineStructure_FloorZero_Returns400);
        await using var ctx = ProjectsDbFixture.NewContext(db);

        var structureService = new ProjectStructureCommandService(
            new UnitRepository(ctx),
            new ProjectRepository(ctx),
            new OutboxMessageRepository(ctx),
            ctx);

        var controller = BuildController(structureService);

        var resource = new DefineStructureResource
        {
            Floors = 3,
            UnitsPerFloor = 1,
            DeviceTypesPerFloor = new List<FloorDeviceTypeSelection>
            {
                new FloorDeviceTypeSelection { Floor = 0, DeviceTypes = new List<string> { "SmartMeter" } }
            }
        };

        var result = await controller.DefineStructure(1, resource);

        (result as BadRequestObjectResult).Should().NotBeNull(
            "floor=0 is always out of range and must return HTTP 400");
    }

    // ── Triangulation: in-range floor references are accepted ───────────────

    [Fact]
    public async Task DefineStructure_InRangeFloor_DoesNotReturn400()
    {
        var db = nameof(DefineStructure_InRangeFloor_DoesNotReturn400);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var structureService = new ProjectStructureCommandService(
            new UnitRepository(ctx),
            new ProjectRepository(ctx),
            new OutboxMessageRepository(ctx),
            ctx);

        var controller = BuildController(structureService);

        var resource = new DefineStructureResource
        {
            Floors = 3,
            UnitsPerFloor = 1,
            DeviceTypesPerFloor = new List<FloorDeviceTypeSelection>
            {
                new FloorDeviceTypeSelection { Floor = 1, DeviceTypes = new List<string> { "SmartMeter" } },
                new FloorDeviceTypeSelection { Floor = 3, DeviceTypes = new List<string> { "WaterSensor" } }
            }
        };

        var result = await controller.DefineStructure(1, resource);

        result.Should().NotBeOfType<BadRequestObjectResult>(
            "floor references within [1, floors] must not be rejected as out-of-range");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ProjectsController BuildController(ProjectStructureCommandService structureService)
    {
        var cmdService = new Mock<IoBuild.Projects.Domain.Services.IProjectCommandService>().Object;
        var qryService = new Mock<IoBuild.Projects.Domain.Services.IProjectQueryService>().Object;

        var controller = new ProjectsController(cmdService, qryService, structureService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        // Simulate Builder role
        controller.HttpContext.Items["UserRole"] = "Builder";
        controller.HttpContext.Items["UserId"] = 1;
        return controller;
    }
}
