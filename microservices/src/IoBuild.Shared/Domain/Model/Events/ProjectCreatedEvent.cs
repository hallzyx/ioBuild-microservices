namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>Published when a Project is persisted for the first time (ADR-4).</summary>
public record ProjectCreatedEvent : DomainEvent
{
    public int ProjectId { get; init; }
    public int BuilderUserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    public override string RoutingKey => "project.project.created";
}
