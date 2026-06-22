using FluentAssertions;
using IoBuild.Devices.Domain.Constants;
using IoBuild.Devices.Interfaces.REST;
using IoBuild.Devices.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Devices.Tests.UnitTests;

/// <summary>
/// Tests for GET /api/v1/devices/types catalog endpoint.
/// Spec: S1.1 (SC-1.1, SC-1.2) — shape, count, stability.
/// TDD: T-B1 (RED written before endpoint exists).
/// </summary>
public class DeviceTypeCatalogControllerTests
{
    // ── SC-1.1: happy path — 200 with exactly 3 entries, each having code + displayName ──

    [Fact]
    public void GetDeviceTypes_Returns200_WithThreeEntries()
    {
        // Arrange
        var controller = new DevicesController(
            commandService: null!,
            queryService: null!);

        // Act
        var result = controller.GetDeviceTypes() as OkObjectResult;

        // Assert
        result.Should().NotBeNull("endpoint must return 200 OK");
        result!.StatusCode.Should().Be(200);

        var body = result.Value as DeviceTypeCatalogResource;
        body.Should().NotBeNull("response body must be a DeviceTypeCatalogResource");
        body!.DeviceTypes.Should().HaveCount(3, "catalog must have exactly 3 device types (SC-1.1)");
    }

    [Fact]
    public void GetDeviceTypes_EachEntry_HasCodeAndDisplayName()
    {
        var controller = new DevicesController(null!, null!);

        var result = (controller.GetDeviceTypes() as OkObjectResult)!;
        var body = (result.Value as DeviceTypeCatalogResource)!;

        body.DeviceTypes.Should().OnlyContain(
            t => !string.IsNullOrWhiteSpace(t.Code) && !string.IsNullOrWhiteSpace(t.DisplayName),
            "every device type entry must have a non-empty code and displayName (SC-1.1)");
    }

    [Fact]
    public void GetDeviceTypes_ContainsSmartMeter_WaterSensor_SmokeDetector()
    {
        var controller = new DevicesController(null!, null!);

        var result = (controller.GetDeviceTypes() as OkObjectResult)!;
        var body = (result.Value as DeviceTypeCatalogResource)!;

        body.DeviceTypes.Select(t => t.Code).Should().BeEquivalentTo(
            new[] { "SmartMeter", "WaterSensor", "SmokeDetector" },
            "catalog must contain the three canonical device type codes (SC-1.1)");
    }

    // ── SC-1.2: stability — two calls return same order ──

    [Fact]
    public void GetDeviceTypes_CalledTwice_ReturnsSameOrder()
    {
        var controller = new DevicesController(null!, null!);

        var first = ((controller.GetDeviceTypes() as OkObjectResult)!.Value as DeviceTypeCatalogResource)!
            .DeviceTypes.Select(t => t.Code).ToList();
        var second = ((controller.GetDeviceTypes() as OkObjectResult)!.Value as DeviceTypeCatalogResource)!
            .DeviceTypes.Select(t => t.Code).ToList();

        first.Should().ContainInOrder(second,
            "catalog order must be deterministic across calls (SC-1.2)");
    }

    // ── Catalog single-source: DevicesController.GetDeviceTypes uses DeviceTypeCatalog.KnownTypes ──

    [Fact]
    public void GetDeviceTypes_Codes_MatchSharedDeviceTypeCatalogKnownTypes()
    {
        var controller = new DevicesController(null!, null!);

        var result = (controller.GetDeviceTypes() as OkObjectResult)!;
        var body = (result.Value as DeviceTypeCatalogResource)!;

        body.DeviceTypes.Select(t => t.Code).Should().BeEquivalentTo(
            IoBuild.Shared.Domain.Model.DeviceTypeCatalog.KnownTypes,
            "catalog endpoint must serve from IoBuild.Shared.DeviceTypeCatalog.KnownTypes " +
            "as the single source of truth (catalog reconciliation requirement)");
    }
}
