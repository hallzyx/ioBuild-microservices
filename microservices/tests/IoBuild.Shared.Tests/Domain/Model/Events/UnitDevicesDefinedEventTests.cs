using System.Text.Json;
using FluentAssertions;
using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Shared.Tests.Domain.Model.Events;

/// <summary>
/// T-05 [RED] → T-06 [GREEN] — UnitDevicesDefinedEvent serialization round-trip + routing-key tests.
/// </summary>
public class UnitDevicesDefinedEventTests
{
    // T-05: RoutingKeyValue constant equals "project.unit.devices.defined"
    [Fact]
    public void RoutingKeyValue_ConstantIsCorrect()
    {
        UnitDevicesDefinedEvent.RoutingKeyValue.Should().Be("project.unit.devices.defined");
    }

    // T-05: instance RoutingKey property must equal the constant
    [Fact]
    public void Instance_RoutingKey_MatchesConstant()
    {
        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 1,
            ProjectId = 2,
            Floor = 1,
            RoomNumber = "01",
            BuilderId = 3,
            DeviceTypes = ["AirConditioner"]
        };

        evt.RoutingKey.Should().Be(UnitDevicesDefinedEvent.RoutingKeyValue);
    }

    // T-05: EventId is non-empty by default
    [Fact]
    public void EventId_IsNonEmptyByDefault()
    {
        var evt = new UnitDevicesDefinedEvent
        {
            UnitId = 42,
            ProjectId = 1,
            Floor = 2,
            RoomNumber = "01",
            BuilderId = 5,
            DeviceTypes = ["AirConditioner", "SmartLight"]
        };

        evt.EventId.Should().NotBeEmpty();
    }

    // T-05: two instances have different EventIds
    [Fact]
    public void TwoInstances_HaveDifferentEventIds()
    {
        var e1 = new UnitDevicesDefinedEvent { UnitId = 1, ProjectId = 1, Floor = 1, RoomNumber = "01", BuilderId = 1, DeviceTypes = [] };
        var e2 = new UnitDevicesDefinedEvent { UnitId = 1, ProjectId = 1, Floor = 1, RoomNumber = "01", BuilderId = 1, DeviceTypes = [] };

        e1.EventId.Should().NotBe(e2.EventId);
    }

    // T-05: JSON round-trip preserves all fields
    [Fact]
    public void JsonRoundTrip_PreservesAllFields()
    {
        var original = new UnitDevicesDefinedEvent
        {
            UnitId = 42,
            ProjectId = 10,
            Floor = 3,
            RoomNumber = "01",
            BuilderId = 7,
            DeviceTypes = ["AirConditioner", "SmartLight"]
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<UnitDevicesDefinedEvent>(json)!;

        restored.EventId.Should().Be(original.EventId);
        restored.UnitId.Should().Be(42);
        restored.ProjectId.Should().Be(10);
        restored.Floor.Should().Be(3);
        restored.RoomNumber.Should().Be("01");
        restored.BuilderId.Should().Be(7);
        restored.DeviceTypes.Should().BeEquivalentTo(["AirConditioner", "SmartLight"]);
        restored.RoutingKey.Should().Be("project.unit.devices.defined");
    }

    // T-05: empty DeviceTypes round-trips as empty (not null)
    [Fact]
    public void JsonRoundTrip_EmptyDeviceTypes_StaysEmpty()
    {
        var original = new UnitDevicesDefinedEvent
        {
            UnitId = 1,
            ProjectId = 1,
            Floor = 1,
            RoomNumber = "01",
            BuilderId = 1,
            DeviceTypes = []
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<UnitDevicesDefinedEvent>(json)!;

        restored.DeviceTypes.Should().NotBeNull();
        restored.DeviceTypes.Should().BeEmpty();
    }
}
