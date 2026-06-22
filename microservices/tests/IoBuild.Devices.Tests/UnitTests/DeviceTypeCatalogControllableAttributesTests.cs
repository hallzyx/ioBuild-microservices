using FluentAssertions;
using IoBuild.Devices.Interfaces.REST;
using IoBuild.Devices.Interfaces.REST.Resources;
using IoBuild.Shared.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace IoBuild.Devices.Tests.UnitTests;

/// <summary>
/// Task 3.1 RED: GET /api/v1/devices/types must include ControllableAttributes populated
/// from DeviceCapabilityCatalog.ByType. Telemetry-only types return empty list.
/// </summary>
public class DeviceTypeCatalogControllableAttributesTests
{
    [Fact]
    public void GetDeviceTypes_Returns_ControllableAttributes_ForAcAndSmartLight()
    {
        var controller = new DevicesController(null!, null!);

        var result = (controller.GetDeviceTypes() as OkObjectResult)!;
        var body = (result.Value as DeviceTypeCatalogResource)!;

        // AirConditioner must have controllable attributes
        var acEntry = body.DeviceTypes.SingleOrDefault(t => t.Code == "AirConditioner");
        acEntry.Should().NotBeNull("AirConditioner must be in the catalog");
        acEntry!.ControllableAttributes.Should().NotBeEmpty(
            "AirConditioner has targetTemperature, mode, power in DeviceCapabilityCatalog");

        var tempAttr = acEntry.ControllableAttributes.SingleOrDefault(a => a.Name == "targetTemperature");
        tempAttr.Should().NotBeNull("targetTemperature must be declared for AirConditioner");
        tempAttr!.Type.Should().Be("number");
        tempAttr.Min.Should().Be(16);
        tempAttr.Max.Should().Be(30);
        tempAttr.Unit.Should().Be("C");

        // SmartLight must have controllable attributes
        var slEntry = body.DeviceTypes.SingleOrDefault(t => t.Code == "SmartLight");
        slEntry.Should().NotBeNull("SmartLight must be in the catalog");
        slEntry!.ControllableAttributes.Should().NotBeEmpty(
            "SmartLight has brightness and power in DeviceCapabilityCatalog");

        var brightnessAttr = slEntry.ControllableAttributes.SingleOrDefault(a => a.Name == "brightness");
        brightnessAttr.Should().NotBeNull("brightness must be declared for SmartLight");
        brightnessAttr!.Min.Should().Be(0);
        brightnessAttr.Max.Should().Be(100);
    }

    [Fact]
    public void GetDeviceTypes_TelemetryOnlyTypes_HaveEmptyControllableAttributes()
    {
        var controller = new DevicesController(null!, null!);

        var result = (controller.GetDeviceTypes() as OkObjectResult)!;
        var body = (result.Value as DeviceTypeCatalogResource)!;

        // Telemetry-only types must have empty ControllableAttributes
        var telemetryTypes = new[] { "SmartMeter", "WaterSensor", "SmokeDetector" };
        foreach (var type in telemetryTypes)
        {
            var entry = body.DeviceTypes.SingleOrDefault(t => t.Code == type);
            entry.Should().NotBeNull($"{type} must be in the catalog");
            entry!.ControllableAttributes.Should().BeEmpty(
                $"{type} is telemetry-only; ControllableAttributes must be empty");
        }
    }
}
