using FluentAssertions;
using IoBuild.Devices.Application.DTOs;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Interfaces.REST;
using IoBuild.Devices.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Interfaces;

/// <summary>
/// C-1 (TDD RED-first): Controller authorization and ownerId resolution.
///
/// Pattern: unit-test the controller directly by setting HttpContext.Items to simulate
/// what JwtAuthenticationMiddleware does in production. This avoids spinning up a
/// WebApplicationFactory (which requires a running app with real JWT config).
///
/// The custom [Authorize] attribute checks HttpContext.Items["User"] for auth guard.
/// Role guard ("Owner" only) is enforced inside the service method — same pattern as
/// DeviceActuationService which reads command.RequestingUserRole.
/// OwnerId is always derived from HttpContext.Items["UserId"] (never from the request body).
/// </summary>
public class CustomDeviceTypeControllerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DevicesDbContext BuildInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new DevicesDbContext(options);
    }

    private static OwnerCustomDeviceTypeAttribute NumberAttr() =>
        new("temperature", "number", Min: 0, Max: 100, Unit: "C");

    private static CustomDeviceTypeController BuildController(
        DevicesDbContext db,
        string? userId = "42",
        string? userRole = "Owner")
    {
        var cmdSvc = new CustomDeviceTypeCommandService(db);
        var querySvc = new CustomDeviceTypeQueryService(db);
        var controller = new CustomDeviceTypeController(cmdSvc, querySvc);

        var httpContext = new DefaultHttpContext();
        if (userId is not null)
        {
            // Simulate what JwtAuthenticationMiddleware sets
            httpContext.Items["UserId"] = int.Parse(userId);
            httpContext.Items["UserRole"] = userRole;
            httpContext.Items["User"] = new { Id = int.Parse(userId), Role = userRole };
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    // ── C-1a: Unauthenticated → 401 ───────────────────────────────────────────
    // The [Authorize] filter blocks before the action runs when Items["User"] is null.
    // We simulate this by testing the filter directly since we're unit-testing without the
    // full middleware pipeline. Instead we test controller behavior when no User set.

    [Fact]
    public async Task CreateCustomType_UnauthenticatedRequest_Returns401()
    {
        await using var db = BuildInMemoryContext(nameof(CreateCustomType_UnauthenticatedRequest_Returns401));

        // No userId / no "User" item → unauthenticated
        var cmdSvc = new CustomDeviceTypeCommandService(db);
        var querySvc = new CustomDeviceTypeQueryService(db);
        var controller = new CustomDeviceTypeController(cmdSvc, querySvc);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()   // no Items["User"]
        };

        var resource = new CreateCustomTypeResource("MySensor", "My Sensor",
            [new CustomTypeAttributeResource("temp", "number", 0, 100, "C", null)]);

        var result = await controller.CreateCustomType(resource);

        result.Should().BeOfType<UnauthorizedObjectResult>("no User in context → 401");
    }

    // ── C-1b: Non-Owner role → 403 ────────────────────────────────────────────

    [Fact]
    public async Task CreateCustomType_BuilderRole_Returns403()
    {
        await using var db = BuildInMemoryContext(nameof(CreateCustomType_BuilderRole_Returns403));
        var controller = BuildController(db, userId: "10", userRole: "Builder");

        var resource = new CreateCustomTypeResource("MySensor", "My Sensor",
            [new CustomTypeAttributeResource("temp", "number", 0, 100, "C", null)]);

        var result = await controller.CreateCustomType(resource);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403, "Builder role must not create custom types");
    }

    // ── C-1c: Owner role → 201 Created ────────────────────────────────────────

    [Fact]
    public async Task CreateCustomType_OwnerRole_Returns201()
    {
        await using var db = BuildInMemoryContext(nameof(CreateCustomType_OwnerRole_Returns201));
        var controller = BuildController(db, userId: "42", userRole: "Owner");

        var resource = new CreateCustomTypeResource("CO2Sensor", "CO2 Sensor",
            [new CustomTypeAttributeResource("co2", "number", 0, 5000, "ppm", null)]);

        var result = await controller.CreateCustomType(resource);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.StatusCode.Should().Be(201);
    }

    // ── C-1d: ownerId resolved from JWT principal, not body ───────────────────
    // The controller ignores any ownerId in the request; it always uses HttpContext.Items["UserId"].

    [Fact]
    public async Task CreateCustomType_OwnerIdFromJwt_NotFromBody()
    {
        await using var db = BuildInMemoryContext(nameof(CreateCustomType_OwnerIdFromJwt_NotFromBody));
        var controller = BuildController(db, userId: "99", userRole: "Owner");

        var resource = new CreateCustomTypeResource("TempSensor", "Temp Sensor",
            [new CustomTypeAttributeResource("temp", "number", 0, 100, "C", null)]);

        await controller.CreateCustomType(resource);

        var stored = await db.OwnerCustomDeviceTypes.FirstAsync();
        stored.OwnerUserId.Should().Be("99", "must use JWT userId, not any body field");
    }

    // ── C-1e: GET — ownerId in query matches principal → 200 ─────────────────

    [Fact]
    public async Task ListCustomTypes_MatchingOwner_Returns200()
    {
        await using var db = BuildInMemoryContext(nameof(ListCustomTypes_MatchingOwner_Returns200));
        var controller = BuildController(db, userId: "42", userRole: "Owner");

        var result = await controller.ListCustomTypes(ownerId: "42");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── C-1f: GET — ownerId in query does NOT match principal → 403 ──────────

    [Fact]
    public async Task ListCustomTypes_MismatchedOwner_Returns403()
    {
        await using var db = BuildInMemoryContext(nameof(ListCustomTypes_MismatchedOwner_Returns403));
        var controller = BuildController(db, userId: "42", userRole: "Owner");

        // Request asks for owner 99's types, but caller is userId 42
        var result = await controller.ListCustomTypes(ownerId: "99");

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }
}
