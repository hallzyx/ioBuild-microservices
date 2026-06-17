namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>Published when a Device is removed and persisted (ADR-4).</summary>
public record DeviceDeletedEvent : DomainEvent
{
    public int DeviceId { get; init; }
    public int OwnerUserId { get; init; }

    public override string RoutingKey => "device.device.deleted";
}
