namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>
/// Marker interface for domain events.
/// Enables eventual consistency between microservices.
/// </summary>
public interface IEvent
{
    DateTime OccurredOn { get; }
}
