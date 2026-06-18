namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>
/// Published by Projects when a Unit's OwnerId is backfilled after an email-based match (REQ-DE-01, §6.1).
/// Consumed by Analytics to update UnitProjection.OwnerUserId.
/// </summary>
public record UnitOwnerMatchedEvent : DomainEvent
{
    public int UnitId { get; init; }
    public int ProjectId { get; init; }
    public int OwnerUserId { get; init; }
    public string OwnerEmail { get; init; } = string.Empty;

    public override string RoutingKey => "project.unit.owner-matched";
}
