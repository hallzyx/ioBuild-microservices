using FluentAssertions;
using IoBuild.Shared.Domain.Model;

namespace IoBuild.Shared.Tests.Domain.Model;

/// <summary>
/// T-01 [RED] — KnownTypes alignment test for AirConditioner + SmartLight.
/// Tests MUST fail until T-02 adds the new types to DeviceTypeCatalog.KnownTypes.
/// </summary>
public class DeviceTypeCatalogTests
{
    // T-01: AirConditioner must be in KnownTypes
    [Fact]
    public void KnownTypes_ContainsAirConditioner()
    {
        DeviceTypeCatalog.KnownTypes.Should().Contain("AirConditioner",
            "AirConditioner is a new controllable device type that must be registered (T-01)");
    }

    // T-01: SmartLight must be in KnownTypes
    [Fact]
    public void KnownTypes_ContainsSmartLight()
    {
        DeviceTypeCatalog.KnownTypes.Should().Contain("SmartLight",
            "SmartLight is a new controllable device type that must be registered (T-01)");
    }

    // T-01: total count must be exactly 5 (3 legacy + 2 new)
    [Fact]
    public void KnownTypes_CountIsExactlyFive()
    {
        DeviceTypeCatalog.KnownTypes.Should().HaveCount(5,
            "KnownTypes must contain exactly 5 types: SmartMeter, WaterSensor, SmokeDetector, AirConditioner, SmartLight");
    }

    // Regression: existing legacy types must still be present
    [Fact]
    public void KnownTypes_StillContainsLegacyTypes()
    {
        DeviceTypeCatalog.KnownTypes.Should().Contain("SmartMeter");
        DeviceTypeCatalog.KnownTypes.Should().Contain("WaterSensor");
        DeviceTypeCatalog.KnownTypes.Should().Contain("SmokeDetector");
    }
}
