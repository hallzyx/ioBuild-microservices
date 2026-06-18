namespace IoBuild.Shared.Domain.Model.Events;

/// <summary>
/// Published when a User is persisted in IAM for the first time (REQ-DE-01, ADR-4).
/// Email is always lower-cased at the publisher (IAM) before the event is emitted.
/// </summary>
public record UserRegisteredEvent : DomainEvent
{
    public int UserId { get; init; }

    /// <summary>User's email address, lower-cased by the publisher (case-insensitive matching contract).</summary>
    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public override string RoutingKey => "iam.user.registered";
}
