namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>Published when a Device is persisted for the first time (ADR-4).</summary>
public record DeviceCreatedEvent : DomainEvent
{
    public int DeviceId { get; init; }
    public int OwnerUserId { get; init; }
    public int? ProjectId { get; init; }
    public int? UnitId { get; init; }
    public string DeviceType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Extended field — PR 2 (§6.2). Null for devices created outside floor provisioning.
    /// <summary>Floor number when provisioned by FloorProvisioningConsumer; null for manually created devices.</summary>
    public int? FloorNumber { get; init; }

    public override string RoutingKey => "device.device.created";
}
