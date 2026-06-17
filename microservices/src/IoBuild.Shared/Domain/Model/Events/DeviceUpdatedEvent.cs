namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>Published when a Device's fields are updated and persisted (ADR-4).</summary>
public record DeviceUpdatedEvent : DomainEvent
{
    public int DeviceId { get; init; }
    public int OwnerUserId { get; init; }
    public int? ProjectId { get; init; }
    public int? UnitId { get; init; }
    public string DeviceType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    public override string RoutingKey => "device.device.updated";
}
