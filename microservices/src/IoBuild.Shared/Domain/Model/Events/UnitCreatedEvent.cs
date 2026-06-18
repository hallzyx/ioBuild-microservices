namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>Published when a Unit is persisted for the first time (ADR-4).</summary>
public record UnitCreatedEvent : DomainEvent
{
    public int UnitId { get; init; }
    public int ProjectId { get; init; }
    public int BuilderUserId { get; init; }
    public int? OwnerUserId { get; init; }
    public string Status { get; init; } = string.Empty;

    // Extended fields — PR 2 (§6.2). Defaults ensure existing producers compile unchanged.
    /// <summary>Floor number of the unit. Default 0 for legacy single-unit creation path.</summary>
    public int Floor { get; init; } = 0;

    /// <summary>Room identifier within the floor. Default empty for legacy paths.</summary>
    public string RoomNumber { get; init; } = string.Empty;

    /// <summary>Owner email pre-assigned at structure definition time, or null.</summary>
    public string? OwnerEmail { get; init; }

    public override string RoutingKey => "project.unit.created";
}
