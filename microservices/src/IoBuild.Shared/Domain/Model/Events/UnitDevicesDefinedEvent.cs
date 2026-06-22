namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>
/// Published once per unit when a define-structure command is processed and that unit has
/// a non-empty device package selected by the builder.
/// Consumed by Devices (UnitDeviceProvisioningConsumer) to provision unit-level devices.
///
/// Keyed by the real UnitId assigned in Phase 1 of the same command (ADR-1/D1).
/// This is a SEPARATE event from FloorStructureDefinedEvent because it carries per-unit
/// identity — one event per unit, not one per floor.
///
/// Routing key: <see cref="RoutingKeyValue"/> = "project.unit.devices.defined".
/// </summary>
public record UnitDevicesDefinedEvent : DomainEvent
{
    /// <summary>
    /// Routing key string constant for queue binding and consumer configuration.
    /// Use <c>UnitDevicesDefinedEvent.RoutingKeyValue</c> when configuring queue bindings
    /// so it can be referenced without an instance.
    /// </summary>
    public const string RoutingKeyValue = "project.unit.devices.defined";

    /// <inheritdoc/>
    public override string RoutingKey => RoutingKeyValue;

    /// <summary>Real DB id of the unit, assigned in Phase 1 of the define-structure command.</summary>
    public int UnitId { get; init; }

    /// <summary>Project the unit belongs to.</summary>
    public int ProjectId { get; init; }

    /// <summary>Floor number — carried so the consumer can set Device.FloorNumber without a cross-service call.</summary>
    public int Floor { get; init; }

    /// <summary>Room identifier within the floor (e.g. "01", "02").</summary>
    public string RoomNumber { get; init; } = string.Empty;

    /// <summary>Builder who submitted the define-structure command.</summary>
    public int BuilderId { get; init; }

    /// <summary>
    /// Device type codes selected for this unit. Validated against DeviceTypeCatalog.KnownTypes
    /// by ProjectStructureCommandService before the event is emitted — no unknown codes here.
    /// </summary>
    public IReadOnlyList<string> DeviceTypes { get; init; } = [];
}
