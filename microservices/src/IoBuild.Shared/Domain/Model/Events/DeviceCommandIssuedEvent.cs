namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>
/// Audit event published when an owner issues a command to a device (ADR-B2, CC-7).
/// Persisted to the Devices outbox table and forwarded to the event bus via OutboxWorker.
///
/// Routing key: <c>device.command.issued</c>.
/// Wire contract body key: <c>attribute</c> (ADR-B5 wins over spec's <c>attributeName</c>).
/// </summary>
public record DeviceCommandIssuedEvent : DomainEvent
{
    /// <summary>ID of the device that received the command.</summary>
    public int DeviceId { get; init; }

    /// <summary>
    /// Catalog attribute name verbatim (e.g. "targetTemperature", "mode", "brightness").
    /// Must match DeviceCapabilityCatalog keys exactly (CC-1).
    /// </summary>
    public string Attribute { get; init; } = string.Empty;

    /// <summary>Serialized value for the attribute (numeric, boolean, or enum string).</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>ID of the owner user who issued the command.</summary>
    public int UserId { get; init; }

    public override string RoutingKey => "device.command.issued";
}
