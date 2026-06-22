using System.Text.Json;
using FluentAssertions;
using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Shared.Tests.Domain.Model.Events;

/// <summary>
/// T-A1 [RED] — FloorStructureDefinedEvent serialization / back-compat tests.
/// Spec: S3.1, S3.2, S3.3.
/// All three tests MUST fail until T-A2 adds the DeviceTypes property.
/// </summary>
public class FloorStructureDefinedEventSerializationTests
{
    // SC-3.1 — round-trip with DeviceTypes = ["WaterSensor"]
    [Fact]
    public void FloorStructureDefinedEvent_DeviceTypes_RoundTrips_WithSingleType()
    {
        var original = new FloorStructureDefinedEvent
        {
            ProjectId = 1,
            Floor = 2,
            UnitCount = 4,
            BuilderId = 7,
            DeviceTypes = new List<string> { "WaterSensor" }
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<FloorStructureDefinedEvent>(json)!;

        restored.DeviceTypes.Should().NotBeNull();
        restored.DeviceTypes!.Should().ContainSingle()
            .Which.Should().Be("WaterSensor");
    }

    // SC-3.2 — legacy payload (no DeviceTypes key) deserializes to null without throwing
    [Fact]
    public void FloorStructureDefinedEvent_LegacyPayload_DeserializesDeviceTypesAsNull()
    {
        // JSON without DeviceTypes key — simulates an in-flight outbox message from before the change
        const string legacyJson =
            """{"EventId":"a1b2c3d4-0000-0000-0000-000000000001","OccurredOn":"2024-01-01T00:00:00Z","ProjectId":5,"Floor":1,"UnitCount":3,"BuilderId":2}""";

        var act = () => JsonSerializer.Deserialize<FloorStructureDefinedEvent>(legacyJson);

        act.Should().NotThrow();
        var restored = act();
        restored!.DeviceTypes.Should().BeNull(
            "legacy payloads without the DeviceTypes key must deserialize to null (S3.2 back-compat)");
    }

    // SC-3.3 — empty list [] deserializes to empty list (not null)
    [Fact]
    public void FloorStructureDefinedEvent_EmptyDeviceTypes_DeserializesAsEmptyList()
    {
        var original = new FloorStructureDefinedEvent
        {
            ProjectId = 3,
            Floor = 1,
            UnitCount = 2,
            BuilderId = 4,
            DeviceTypes = new List<string>()
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<FloorStructureDefinedEvent>(json)!;

        restored.DeviceTypes.Should().NotBeNull(
            "an explicitly-set empty list must round-trip as an empty list, not null (S3.3)");
        restored.DeviceTypes!.Should().BeEmpty();
    }
}
