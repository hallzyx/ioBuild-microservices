namespace IoBuild.Projects.Domain.Model.Entities;

/// <summary>
/// Local read-model mirror of an IAM owner identity (§3.3, ADR-B).
///
/// Projects keeps this table populated exclusively from <c>UserRegisteredEvent</c> messages
/// (never via a synchronous IAM call). It is used by the unit-first matching path in
/// <see cref="IoBuild.Projects.Application.Services.ProjectStructureCommandService"/>:
/// when a Unit.OwnerEmail is assigned during structure definition, a lookup against
/// this table immediately sets <c>Unit.OwnerId</c> if the owner already registered.
///
/// Email invariant (ADR-D): <see cref="Email"/> is always stored lower-cased.
/// Table: <c>registered_owner</c>.
/// </summary>
public class RegisteredOwner
{
    /// <summary>Primary key — lower-cased owner email (ADR-D).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>IAM UserId assigned to this email.</summary>
    public int UserId { get; private set; }

    /// <summary>LWW guard — timestamp of the most recent event that populated this row.</summary>
    public DateTime LastEventAt { get; private set; }

    /// <summary>EF Core parameterless constructor — required for entity materialisation.</summary>
    protected RegisteredOwner() { }

    /// <summary>Creates a new mirror row. Email is lower-cased (ADR-D).</summary>
    public RegisteredOwner(string email, int userId, DateTime lastEventAt)
    {
        Email = email.ToLowerInvariant();
        UserId = userId;
        LastEventAt = lastEventAt;
    }

    /// <summary>
    /// Updates this row with fresher data from a redelivered or newer event (LWW).
    /// Only applied when <paramref name="eventAt"/> is newer than <see cref="LastEventAt"/>.
    /// </summary>
    public void UpdateIfNewer(int userId, DateTime eventAt)
    {
        if (eventAt <= LastEventAt)
            return;
        UserId = userId;
        LastEventAt = eventAt;
    }
}
