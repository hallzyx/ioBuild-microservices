using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Model;

namespace IoBuild.Devices.Tests.Domain;

/// <summary>
/// DT-1 (TDD RED-first): Domain guard tests for DeviceType catalog aggregate.
/// Pure domain — no DB needed.
///
/// Design D1 divergence from spec DT-1-S7:
///   Spec says "at least one attribute MUST be present (empty list rejected)".
///   Design D1 OVERRIDES: telemetry-only types MAY have zero attributes.
///   Test DT-1-S7-DESIGN-OVERRIDE below asserts zero attributes ARE allowed.
/// </summary>
public class DeviceTypeTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DeviceCapabilityCatalog.ControllableAttribute NumberAttr() =>
        new("targetTemperature", "number", 16, 30, "C");

    private static DeviceCapabilityCatalog.ControllableAttribute BoolAttr() =>
        new("power", "boolean", null, null, null);

    private static DeviceCapabilityCatalog.ControllableAttribute EnumAttr() =>
        new("mode", "enum", null, null, null, ["cooling", "heating", "fan"]);

    // ── DT-1-S1: Valid aggregate with 3 attrs ─────────────────────────────────

    [Fact]
    public void Constructor_ValidArgs_ThreeAttrs_SetsAllFields()
    {
        var attrs = new[] { NumberAttr(), EnumAttr(), BoolAttr() };

        var sut = new DeviceType("AirConditioner", "Air Conditioner", "unit", attrs);

        sut.Code.Should().Be("AirConditioner");
        sut.DisplayName.Should().Be("Air Conditioner");
        sut.Scope.Should().Be("unit");
        sut.GetAttributes().Should().HaveCount(3);
    }

    // ── DT-1-S2: Empty Code rejected ─────────────────────────────────────────

    [Fact]
    public void Constructor_EmptyCode_Throws()
    {
        var attrs = new[] { NumberAttr() };

        var act = () => new DeviceType("", "Air Conditioner", "unit", attrs);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Code*");
    }

    // ── DT-1-S3: DisplayName > 100 chars rejected ─────────────────────────────

    [Fact]
    public void Constructor_DisplayNameTooLong_Throws()
    {
        var longName = new string('x', 101);
        var attrs = new[] { NumberAttr() };

        var act = () => new DeviceType("AC", longName, "unit", attrs);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*displayName*");
    }

    // ── DT-1-S4: Number attribute without unit rejected ───────────────────────

    [Fact]
    public void Constructor_NumberAttributeWithoutUnit_Throws()
    {
        var badAttr = new DeviceCapabilityCatalog.ControllableAttribute(
            "temp", "number", 0, 100, null);

        var act = () => new DeviceType("AC", "AC", "unit", [badAttr]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*unit*");
    }

    // ── DT-1-S5: Number attribute without min/max rejected ───────────────────

    [Fact]
    public void Constructor_NumberAttributeWithoutMin_Throws()
    {
        var badAttr = new DeviceCapabilityCatalog.ControllableAttribute(
            "temp", "number", null, 100, "C");

        var act = () => new DeviceType("AC", "AC", "unit", [badAttr]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*min*");
    }

    // ── DT-1-S6: Enum attribute with fewer than 2 members rejected ────────────

    [Fact]
    public void Constructor_EnumAttributeWithOneMember_Throws()
    {
        var badAttr = new DeviceCapabilityCatalog.ControllableAttribute(
            "mode", "enum", null, null, null, ["cooling"]);

        var act = () => new DeviceType("AC", "AC", "unit", [badAttr]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*enum*");
    }

    // ── DT-1-S7-DESIGN-OVERRIDE: Zero attrs ALLOWED for telemetry-only types ──
    // Spec DT-1-S7 says "empty list rejected".
    // Design D1 OVERRIDES: telemetry-only types have zero attrs — this MUST pass.
    // Tasks artifact encodes this mismatch in a comment; follow design D1/R3.

    [Fact]
    public void Constructor_ZeroAttributes_AllowedForTelemetryOnlyType()
    {
        // SmartMeter is a telemetry-only type with zero controllable attributes
        var act = () => new DeviceType("SmartMeter", "Smart Meter", "floor", []);

        act.Should().NotThrow("zero attributes are allowed per design D1 for telemetry-only types");
    }

    // ── GetAttributes round-trip ──────────────────────────────────────────────

    [Fact]
    public void GetAttributes_RoundTrips_AllAttributeKinds()
    {
        var attrs = new[]
        {
            NumberAttr(),
            BoolAttr(),
            EnumAttr(),
        };

        var sut = new DeviceType("AC", "Air Conditioner", "unit", attrs);

        var result = sut.GetAttributes();
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("targetTemperature");
        result[0].Type.Should().Be("number");
        result[0].Min.Should().Be(16);
        result[0].Max.Should().Be(30);
        result[0].Unit.Should().Be("C");
        result[1].Name.Should().Be("power");
        result[1].Type.Should().Be("boolean");
        result[2].Name.Should().Be("mode");
        result[2].Type.Should().Be("enum");
        result[2].EnumMembers.Should().BeEquivalentTo(["cooling", "heating", "fan"]);
    }

    // ── GetAttributes returns empty list for telemetry-only type ─────────────

    [Fact]
    public void GetAttributes_TelemetryOnlyType_ReturnsEmptyList()
    {
        var sut = new DeviceType("SmartMeter", "Smart Meter", "floor", []);

        sut.GetAttributes().Should().BeEmpty();
    }
}
