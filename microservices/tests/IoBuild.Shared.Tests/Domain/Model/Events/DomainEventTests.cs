using IoBuild.Shared.Domain.Model.Events;
using FluentAssertions;

namespace IoBuild.Shared.Tests.Domain.Model.Events;

/// <summary>
/// RED tests for task 1.1 — domain event contracts.
/// All tests MUST fail until Phase 3 implementation is complete.
/// </summary>
public class DomainEventTests
{
    [Fact]
    public void DeviceCreatedEvent_ImplementsIEvent()
    {
        var evt = new DeviceCreatedEvent
        {
            DeviceId = 1,
            OwnerUserId = 10,
            DeviceType = "Sensor",
            Status = "Online"
        };

        evt.Should().BeAssignableTo<IEvent>();
    }

    [Fact]
    public void DeviceCreatedEvent_HasNonEmptyEventId()
    {
        var evt = new DeviceCreatedEvent { DeviceId = 1, OwnerUserId = 1, DeviceType = "T", Status = "Online" };

        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void DeviceCreatedEvent_OccurredOnIsUtc()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var evt = new DeviceCreatedEvent { DeviceId = 1, OwnerUserId = 1, DeviceType = "T", Status = "Online" };
        var after = DateTime.UtcNow.AddSeconds(1);

        evt.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
        evt.OccurredOn.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void DeviceCreatedEvent_HasCorrectRoutingKey()
    {
        var evt = new DeviceCreatedEvent { DeviceId = 1, OwnerUserId = 1, DeviceType = "T", Status = "Online" };

        evt.RoutingKey.Should().Be("device.device.created");
    }

    [Fact]
    public void DeviceUpdatedEvent_HasCorrectRoutingKey()
    {
        var evt = new DeviceUpdatedEvent { DeviceId = 1, OwnerUserId = 1, DeviceType = "T", Status = "Online" };

        evt.RoutingKey.Should().Be("device.device.updated");
    }

    [Fact]
    public void DeviceDeletedEvent_HasCorrectRoutingKey()
    {
        var evt = new DeviceDeletedEvent { DeviceId = 1, OwnerUserId = 1 };

        evt.RoutingKey.Should().Be("device.device.deleted");
    }

    [Fact]
    public void ProjectCreatedEvent_ImplementsIEvent()
    {
        var evt = new ProjectCreatedEvent
        {
            ProjectId = 1,
            BuilderUserId = 5,
            Name = "Test",
            Status = "OnGoing"
        };

        evt.Should().BeAssignableTo<IEvent>();
    }

    [Fact]
    public void ProjectCreatedEvent_HasNonEmptyEventId()
    {
        var evt = new ProjectCreatedEvent { ProjectId = 1, BuilderUserId = 5, Name = "Test", Status = "OnGoing" };

        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void ProjectCreatedEvent_OccurredOnIsUtc()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var evt = new ProjectCreatedEvent { ProjectId = 1, BuilderUserId = 5, Name = "Test", Status = "OnGoing" };
        var after = DateTime.UtcNow.AddSeconds(1);

        evt.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
        evt.OccurredOn.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void ProjectCreatedEvent_HasCorrectRoutingKey()
    {
        var evt = new ProjectCreatedEvent { ProjectId = 1, BuilderUserId = 5, Name = "Test", Status = "OnGoing" };

        evt.RoutingKey.Should().Be("project.project.created");
    }

    [Fact]
    public void ProjectUpdatedEvent_HasCorrectRoutingKey()
    {
        var evt = new ProjectUpdatedEvent { ProjectId = 1, BuilderUserId = 5, Name = "Test", Status = "OnGoing" };

        evt.RoutingKey.Should().Be("project.project.updated");
    }

    [Fact]
    public void UnitCreatedEvent_ImplementsIEvent()
    {
        var evt = new UnitCreatedEvent
        {
            UnitId = 1,
            ProjectId = 2,
            BuilderUserId = 5,
            Status = "Vacant"
        };

        evt.Should().BeAssignableTo<IEvent>();
    }

    [Fact]
    public void UnitCreatedEvent_HasNonEmptyEventId()
    {
        var evt = new UnitCreatedEvent { UnitId = 1, ProjectId = 2, BuilderUserId = 5, Status = "Vacant" };

        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void UnitCreatedEvent_HasCorrectRoutingKey()
    {
        var evt = new UnitCreatedEvent { UnitId = 1, ProjectId = 2, BuilderUserId = 5, Status = "Vacant" };

        evt.RoutingKey.Should().Be("project.unit.created");
    }

    [Fact]
    public void TwoEventsHaveDifferentEventIds()
    {
        var e1 = new DeviceCreatedEvent { DeviceId = 1, OwnerUserId = 1, DeviceType = "T", Status = "Online" };
        var e2 = new DeviceCreatedEvent { DeviceId = 1, OwnerUserId = 1, DeviceType = "T", Status = "Online" };

        e1.EventId.Should().NotBe(e2.EventId);
    }

    [Fact]
    public void DomainEventBase_IsAbstract()
    {
        typeof(DomainEvent).IsAbstract.Should().BeTrue();
    }
}
