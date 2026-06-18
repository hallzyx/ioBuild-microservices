using System.Text.Json;
using FluentAssertions;
using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Shared.Tests.Domain.Model.Events;

/// <summary>
/// Serialization round-trip tests for PR 2 — Shared Event Contracts.
/// Tasks 2.1, 2.2, 2.3 (RED → GREEN).
/// </summary>
public class SharedEventContractTests
{
    // ─── Task 2.1: UserRegisteredEvent ──────────────────────────────────────

    [Fact]
    public void UserRegisteredEvent_HasNonEmptyEventId()
    {
        var evt = new UserRegisteredEvent { UserId = 1, Email = "alice@example.com", Role = "Owner" };

        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void UserRegisteredEvent_HasCorrectRoutingKey()
    {
        var evt = new UserRegisteredEvent { UserId = 1, Email = "alice@example.com", Role = "Owner" };

        evt.RoutingKey.Should().Be("iam.user.registered");
    }

    [Fact]
    public void UserRegisteredEvent_JsonRoundTrip_PreservesAllFields()
    {
        var original = new UserRegisteredEvent
        {
            UserId = 42,
            Email = "alice@example.com",
            Role = "Owner"
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<UserRegisteredEvent>(json)!;

        restored.EventId.Should().Be(original.EventId);
        restored.OccurredOn.Should().BeCloseTo(original.OccurredOn, TimeSpan.FromMilliseconds(10));
        restored.UserId.Should().Be(42);
        restored.Email.Should().Be("alice@example.com");
        restored.Role.Should().Be("Owner");
        restored.RoutingKey.Should().Be("iam.user.registered");
    }

    // ─── Task 2.1: UnitOwnerMatchedEvent ────────────────────────────────────

    [Fact]
    public void UnitOwnerMatchedEvent_HasNonEmptyEventId()
    {
        var evt = new UnitOwnerMatchedEvent
        {
            UnitId = 1, ProjectId = 2, OwnerUserId = 3, OwnerEmail = "bob@example.com"
        };

        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void UnitOwnerMatchedEvent_HasCorrectRoutingKey()
    {
        var evt = new UnitOwnerMatchedEvent
        {
            UnitId = 1, ProjectId = 2, OwnerUserId = 3, OwnerEmail = "bob@example.com"
        };

        evt.RoutingKey.Should().Be("project.unit.owner-matched");
    }

    [Fact]
    public void UnitOwnerMatchedEvent_JsonRoundTrip_PreservesAllFields()
    {
        var original = new UnitOwnerMatchedEvent
        {
            UnitId = 10,
            ProjectId = 20,
            OwnerUserId = 42,
            OwnerEmail = "bob@example.com"
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<UnitOwnerMatchedEvent>(json)!;

        restored.EventId.Should().Be(original.EventId);
        restored.UnitId.Should().Be(10);
        restored.ProjectId.Should().Be(20);
        restored.OwnerUserId.Should().Be(42);
        restored.OwnerEmail.Should().Be("bob@example.com");
        restored.RoutingKey.Should().Be("project.unit.owner-matched");
    }

    // ─── Task 2.1: FloorStructureDefinedEvent ───────────────────────────────

    [Fact]
    public void FloorStructureDefinedEvent_HasNonEmptyEventId()
    {
        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1, Floor = 2, UnitCount = 5, BuilderId = 3
        };

        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void FloorStructureDefinedEvent_HasCorrectRoutingKey()
    {
        var evt = new FloorStructureDefinedEvent
        {
            ProjectId = 1, Floor = 2, UnitCount = 5, BuilderId = 3
        };

        evt.RoutingKey.Should().Be("project.floor.defined");
    }

    [Fact]
    public void FloorStructureDefinedEvent_JsonRoundTrip_PreservesAllFields()
    {
        var original = new FloorStructureDefinedEvent
        {
            ProjectId = 100,
            Floor = 3,
            UnitCount = 8,
            BuilderId = 7
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<FloorStructureDefinedEvent>(json)!;

        restored.EventId.Should().Be(original.EventId);
        restored.ProjectId.Should().Be(100);
        restored.Floor.Should().Be(3);
        restored.UnitCount.Should().Be(8);
        restored.BuilderId.Should().Be(7);
        restored.RoutingKey.Should().Be("project.floor.defined");
    }

    // ─── Task 2.2: UnitCreatedEvent extended fields ──────────────────────────

    [Fact]
    public void UnitCreatedEvent_NewFields_Serialize_Correctly()
    {
        var original = new UnitCreatedEvent
        {
            UnitId = 5,
            ProjectId = 2,
            BuilderUserId = 10,
            Status = "Vacant",
            Floor = 3,
            RoomNumber = "301",
            OwnerEmail = "carol@example.com"
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<UnitCreatedEvent>(json)!;

        restored.Floor.Should().Be(3);
        restored.RoomNumber.Should().Be("301");
        restored.OwnerEmail.Should().Be("carol@example.com");
    }

    [Fact]
    public void UnitCreatedEvent_ExistingFields_Unchanged()
    {
        var original = new UnitCreatedEvent
        {
            UnitId = 1,
            ProjectId = 2,
            BuilderUserId = 5,
            OwnerUserId = 9,
            Status = "Vacant"
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<UnitCreatedEvent>(json)!;

        restored.UnitId.Should().Be(1);
        restored.ProjectId.Should().Be(2);
        restored.BuilderUserId.Should().Be(5);
        restored.OwnerUserId.Should().Be(9);
        restored.Status.Should().Be("Vacant");
        restored.RoutingKey.Should().Be("project.unit.created");
    }

    [Fact]
    public void UnitCreatedEvent_OwnerEmail_NullByDefault()
    {
        var evt = new UnitCreatedEvent
        {
            UnitId = 1, ProjectId = 2, BuilderUserId = 5, Status = "Vacant"
        };

        evt.OwnerEmail.Should().BeNull();
    }

    [Fact]
    public void UnitCreatedEvent_Floor_DefaultsToZero()
    {
        var evt = new UnitCreatedEvent
        {
            UnitId = 1, ProjectId = 2, BuilderUserId = 5, Status = "Vacant"
        };

        evt.Floor.Should().Be(0);
    }

    [Fact]
    public void UnitCreatedEvent_RoomNumber_DefaultsToEmpty()
    {
        var evt = new UnitCreatedEvent
        {
            UnitId = 1, ProjectId = 2, BuilderUserId = 5, Status = "Vacant"
        };

        evt.RoomNumber.Should().Be(string.Empty);
    }

    // ─── Task 2.3: DeviceCreatedEvent FloorNumber? ───────────────────────────

    [Fact]
    public void DeviceCreatedEvent_FloorNumber_SerializesAsNull_WhenNotSet()
    {
        var original = new DeviceCreatedEvent
        {
            DeviceId = 1,
            OwnerUserId = 2,
            DeviceType = "SmartMeter",
            Status = "Active"
        };

        var json = JsonSerializer.Serialize(original, original.GetType());

        // FloorNumber must be present as null in JSON (not missing) or absent — either is fine
        // What matters is that deserialized value is null.
        var restored = JsonSerializer.Deserialize<DeviceCreatedEvent>(json)!;
        restored.FloorNumber.Should().BeNull();
    }

    [Fact]
    public void DeviceCreatedEvent_FloorNumber_RoundTrips_WhenSet()
    {
        var original = new DeviceCreatedEvent
        {
            DeviceId = 1,
            OwnerUserId = 2,
            DeviceType = "SmartMeter",
            Status = "Active",
            FloorNumber = 4
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<DeviceCreatedEvent>(json)!;

        restored.FloorNumber.Should().Be(4);
    }

    [Fact]
    public void DeviceCreatedEvent_ExistingFields_Unchanged()
    {
        var original = new DeviceCreatedEvent
        {
            DeviceId = 7,
            OwnerUserId = 3,
            ProjectId = 99,
            UnitId = 5,
            DeviceType = "WaterSensor",
            Status = "Online",
            FloorNumber = 2
        };

        var json = JsonSerializer.Serialize(original, original.GetType());
        var restored = JsonSerializer.Deserialize<DeviceCreatedEvent>(json)!;

        restored.DeviceId.Should().Be(7);
        restored.OwnerUserId.Should().Be(3);
        restored.ProjectId.Should().Be(99);
        restored.UnitId.Should().Be(5);
        restored.DeviceType.Should().Be("WaterSensor");
        restored.Status.Should().Be("Online");
        restored.RoutingKey.Should().Be("device.device.created");
    }
}
