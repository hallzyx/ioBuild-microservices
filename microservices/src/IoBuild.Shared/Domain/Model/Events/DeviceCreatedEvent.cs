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

    // Extended field — PR owner-custom-device-type. Null for legacy/catalog devices with no stored name.
    /// <summary>
    /// User-given display name for the device. Set for OwnerCustom devices; null for legacy and
    /// floor/unit provisioned devices (those rely on the fallback format at the query layer).
    /// Old events that don't carry this field deserialize to null cleanly (nullable init).
    /// </summary>
    public string? DeviceName { get; init; }

    public override string RoutingKey => "device.device.created";
}
