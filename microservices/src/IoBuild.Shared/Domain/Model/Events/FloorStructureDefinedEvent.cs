namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>
/// Published once per floor when a define-structure command is processed (REQ-DE-01, §6.1).
/// Consumed by Devices to provision default floor devices (FloorProvisioningConsumer).
/// </summary>
public record FloorStructureDefinedEvent : DomainEvent
{
    public int ProjectId { get; init; }
    public int Floor { get; init; }
    public int UnitCount { get; init; }
    public int BuilderId { get; init; }

    /// <summary>
    /// Per-floor device types selected by the builder (S3.1).
    /// Nullable — absent in legacy payloads. A null or empty value means "use legacy defaults"
    /// (SmartMeter, WaterSensor, SmokeDetector). Additive, backward-compatible field.
    /// </summary>
    public IReadOnlyList<string>? DeviceTypes { get; init; }

    public override string RoutingKey => "project.floor.defined";
}
