namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>Published when a Unit is persisted for the first time (ADR-4).</summary>
public record UnitCreatedEvent : DomainEvent
{
    public int UnitId { get; init; }
    public int ProjectId { get; init; }
    public int BuilderUserId { get; init; }
    public int? OwnerUserId { get; init; }
    public string Status { get; init; } = string.Empty;

    public override string RoutingKey => "project.unit.created";
}
