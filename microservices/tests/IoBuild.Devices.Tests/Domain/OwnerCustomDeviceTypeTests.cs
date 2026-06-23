using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;

namespace IoBuild.Devices.Tests.Domain;

/// <summary>
/// A-1 (TDD RED-first): Domain guard tests for OwnerCustomDeviceType aggregate.
/// Pure domain — no DB needed.
/// </summary>
public class OwnerCustomDeviceTypeTests
{
    // ── Helper: minimal valid attribute set ───────────────────────────────────

    private static OwnerCustomDeviceTypeAttribute NumberAttr() =>
        new("temperature", "number", Min: 0, Max: 100, Unit: "C");

    private static OwnerCustomDeviceTypeAttribute BoolAttr() =>
        new("power", "boolean");

    private static OwnerCustomDeviceTypeAttribute EnumAttr() =>
        new("mode", "enum", EnumMembers: ["cooling", "heating"]);

    // ── A-1a: Valid construction sets all fields ──────────────────────────────

    [Fact]
    public void Constructor_ValidArgs_SetsAllFields()
    {
        var attrs = new[] { NumberAttr() };

        var sut = new OwnerCustomDeviceType("owner-42", "CO2Monitor", "CO2 Monitor", attrs);

        sut.OwnerUserId.Should().Be("owner-42");
        sut.TypeCode.Should().Be("CO2Monitor");
        sut.DisplayName.Should().Be("CO2 Monitor");
        sut.AttributesJson.Should().NotBeNullOrEmpty();
    }

    // ── A-1b: Zero attributes throws ─────────────────────────────────────────

    [Fact]
    public void Constructor_ZeroAttributes_Throws()
    {
        var act = () => new OwnerCustomDeviceType("owner-1", "T1", "Type One", []);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one*");
    }

    // ── A-1c: Empty displayName throws ───────────────────────────────────────

    [Fact]
    public void Constructor_EmptyDisplayName_Throws()
    {
        var attrs = new[] { NumberAttr() };

        var act = () => new OwnerCustomDeviceType("owner-1", "T1", "", attrs);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*displayName*");
    }

    // ── A-1d: DisplayName > 100 chars throws ─────────────────────────────────

    [Fact]
    public void Constructor_DisplayNameTooLong_Throws()
    {
        var attrs = new[] { NumberAttr() };
        var longName = new string('x', 101);

        var act = () => new OwnerCustomDeviceType("owner-1", "T1", longName, attrs);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*displayName*");
    }

    // ── A-1e: Number attribute without unit throws ────────────────────────────

    [Fact]
    public void Constructor_NumberAttributeWithoutUnit_Throws()
    {
        var badAttr = new OwnerCustomDeviceTypeAttribute("temp", "number", Min: 0, Max: 100, Unit: null);

        var act = () => new OwnerCustomDeviceType("owner-1", "T1", "Type One", [badAttr]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*unit*");
    }

    // ── A-1f: Enum attribute with fewer than 2 members throws ─────────────────

    [Fact]
    public void Constructor_EnumAttributeWithOneMember_Throws()
    {
        var badAttr = new OwnerCustomDeviceTypeAttribute("mode", "enum", EnumMembers: ["only-one"]);

        var act = () => new OwnerCustomDeviceType("owner-1", "T1", "Type One", [badAttr]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*enum*");
    }

    // ── A-1g: Boolean attr — valid (no min/max/unit/enum) ─────────────────────

    [Fact]
    public void Constructor_BooleanAttribute_IsValid()
    {
        var attrs = new[] { BoolAttr() };

        var act = () => new OwnerCustomDeviceType("owner-1", "T1", "Type One", attrs);

        act.Should().NotThrow();
    }

    // ── A-1h: Enum attribute with 2+ members — valid ──────────────────────────

    [Fact]
    public void Constructor_EnumAttributeWithTwoMembers_IsValid()
    {
        var attrs = new[] { EnumAttr() };

        var act = () => new OwnerCustomDeviceType("owner-1", "T1", "Type One", attrs);

        act.Should().NotThrow();
    }

    // ── A-1i: GetAttributes() round-trips attribute JSON ──────────────────────

    [Fact]
    public void GetAttributes_RoundTrips_NumberAttribute()
    {
        var original = NumberAttr();
        var sut = new OwnerCustomDeviceType("owner-1", "T1", "Type One", [original]);

        var attrs = sut.GetAttributes();

        attrs.Should().HaveCount(1);
        attrs[0].Name.Should().Be("temperature");
        attrs[0].Type.Should().Be("number");
        attrs[0].Min.Should().Be(0);
        attrs[0].Max.Should().Be(100);
        attrs[0].Unit.Should().Be("C");
    }
}
