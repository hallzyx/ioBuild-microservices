using FluentAssertions;
using IoBuild.Shared.Domain.Model;

namespace IoBuild.Shared.Tests.Domain.Model;

/// <summary>
/// T-03 [RED] — DeviceCapabilityCatalog metadata tests.
/// Tests MUST fail until T-04 creates DeviceCapabilityCatalog.
/// </summary>
public class DeviceCapabilityCatalogTests
{
    // ── AirConditioner ──────────────────────────────────────────────────────

    [Fact]
    public void AirConditioner_IsControllable()
    {
        DeviceCapabilityCatalog.IsControllable("AirConditioner").Should().BeTrue(
            "AirConditioner is a controllable device type");
    }

    [Fact]
    public void AirConditioner_ByType_HasEntry()
    {
        DeviceCapabilityCatalog.ByType.Should().ContainKey("AirConditioner");
    }

    [Fact]
    public void AirConditioner_ControllableAttributes_ContainsTargetTemperature()
    {
        var capability = DeviceCapabilityCatalog.ByType["AirConditioner"];

        capability.ControllableAttributes.Should().Contain(a =>
            a.Name == "targetTemperature" &&
            a.Type == "number" &&
            a.Min == 16 &&
            a.Max == 30 &&
            a.Unit == "C",
            "AirConditioner must declare targetTemperature (number, 16-30, C)");
    }

    [Fact]
    public void AirConditioner_ControllableAttributes_ContainsMode()
    {
        var capability = DeviceCapabilityCatalog.ByType["AirConditioner"];

        capability.ControllableAttributes.Should().Contain(a =>
            a.Name == "mode" && a.Type == "enum",
            "AirConditioner must declare mode (enum) attribute");
    }

    [Fact]
    public void AirConditioner_ControllableAttributes_ContainsPower()
    {
        var capability = DeviceCapabilityCatalog.ByType["AirConditioner"];

        capability.ControllableAttributes.Should().Contain(a =>
            a.Name == "power" && a.Type == "boolean",
            "AirConditioner must declare power (boolean) attribute");
    }

    // ── SmartLight ──────────────────────────────────────────────────────────

    [Fact]
    public void SmartLight_IsControllable()
    {
        DeviceCapabilityCatalog.IsControllable("SmartLight").Should().BeTrue(
            "SmartLight is a controllable device type");
    }

    [Fact]
    public void SmartLight_ByType_HasEntry()
    {
        DeviceCapabilityCatalog.ByType.Should().ContainKey("SmartLight");
    }

    [Fact]
    public void SmartLight_ControllableAttributes_ContainsBrightness()
    {
        var capability = DeviceCapabilityCatalog.ByType["SmartLight"];

        capability.ControllableAttributes.Should().Contain(a =>
            a.Name == "brightness" &&
            a.Type == "number" &&
            a.Min == 0 &&
            a.Max == 100 &&
            a.Unit == "%",
            "SmartLight must declare brightness (number, 0-100, %)");
    }

    [Fact]
    public void SmartLight_ControllableAttributes_ContainsPower()
    {
        var capability = DeviceCapabilityCatalog.ByType["SmartLight"];

        capability.ControllableAttributes.Should().Contain(a =>
            a.Name == "power" && a.Type == "boolean",
            "SmartLight must declare power (boolean) attribute");
    }

    // ── Telemetry-only types (SmartMeter, WaterSensor, SmokeDetector) ───────

    [Fact]
    public void SmartMeter_NotInByType()
    {
        DeviceCapabilityCatalog.ByType.Should().NotContainKey("SmartMeter",
            "SmartMeter is telemetry-only and has zero controllable attributes");
    }

    [Fact]
    public void WaterSensor_NotInByType()
    {
        DeviceCapabilityCatalog.ByType.Should().NotContainKey("WaterSensor",
            "WaterSensor is telemetry-only and has zero controllable attributes");
    }

    [Fact]
    public void SmokeDetector_NotInByType()
    {
        DeviceCapabilityCatalog.ByType.Should().NotContainKey("SmokeDetector",
            "SmokeDetector is telemetry-only and has zero controllable attributes");
    }

    [Fact]
    public void SmartMeter_IsNotControllable()
    {
        DeviceCapabilityCatalog.IsControllable("SmartMeter").Should().BeFalse();
    }

    [Fact]
    public void WaterSensor_IsNotControllable()
    {
        DeviceCapabilityCatalog.IsControllable("WaterSensor").Should().BeFalse();
    }

    [Fact]
    public void SmokeDetector_IsNotControllable()
    {
        DeviceCapabilityCatalog.IsControllable("SmokeDetector").Should().BeFalse();
    }

    // ── Alignment: every ByType key must be in KnownTypes ───────────────────

    [Fact]
    public void Alignment_EveryCapabilityKeyExistsInKnownTypes()
    {
        foreach (var type in DeviceCapabilityCatalog.ByType.Keys)
        {
            DeviceTypeCatalog.KnownTypes.Should().Contain(type,
                $"DeviceCapabilityCatalog key '{type}' must exist in DeviceTypeCatalog.KnownTypes (alignment test)");
        }
    }
}
