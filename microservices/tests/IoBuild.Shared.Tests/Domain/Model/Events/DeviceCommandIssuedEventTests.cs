using System.Text.Json;
using FluentAssertions;
using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Shared.Tests.Domain.Model.Events;

/// <summary>
/// Contract tests for DeviceCommandIssuedEvent (task 1.6 RED → GREEN).
/// Routing key: "device.command.issued". Fields: DeviceId, Attribute, Value, UserId, OccurredAt.
/// </summary>
public class DeviceCommandIssuedEventTests
{
    [Fact]
    public void DeviceCommandIssuedEvent_HasNonEmptyEventId()
    {
        var evt = new DeviceCommandIssuedEvent
        {
            DeviceId = 1,
            Attribute = "targetTemperature",
            Value = "22",
            UserId = 5
        };

        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void DeviceCommandIssuedEvent_HasCorrectRoutingKey()
    {
        var evt = new DeviceCommandIssuedEvent
        {
            DeviceId = 1,
            Attribute = "targetTemperature",
            Value = "22",
            UserId = 5
        };

        evt.RoutingKey.Should().Be("device.command.issued");
    }

    [Fact]
    public void DeviceCommandIssuedEvent_JsonRoundTrip_PreservesAllFields()
    {
        var original = new DeviceCommandIssuedEvent
        {
            DeviceId = 42,
            Attribute = "brightness",
            Value = "80",
            UserId = 7
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<DeviceCommandIssuedEvent>(json)!;

        restored.EventId.Should().Be(original.EventId);
        restored.DeviceId.Should().Be(42);
        restored.Attribute.Should().Be("brightness");
        restored.Value.Should().Be("80");
        restored.UserId.Should().Be(7);
        restored.RoutingKey.Should().Be("device.command.issued");
    }

    [Fact]
    public void DeviceCommandIssuedEvent_OccurredAt_IsUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var evt = new DeviceCommandIssuedEvent
        {
            DeviceId = 1,
            Attribute = "mode",
            Value = "cooling",
            UserId = 3
        };
        var after = DateTime.UtcNow.AddSeconds(1);

        evt.OccurredOn.Should().BeAfter(before).And.BeBefore(after);
    }
}
