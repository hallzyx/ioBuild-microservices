namespace IoBuild.Projects.Domain.Model.Aggregates;

/// <summary>
/// Unit aggregate (§1.1 design.md — PR 3).
///
/// Shape change: Floor, RoomNumber, OwnerEmail added; OwnerId changed to int?
/// (D3 — nullable so a unit can exist without an assigned owner, REQ-PS-02).
///
/// Email invariant (ADR-D): all emails stored lower-cased so MySQL CI collation and
/// EF-InMemory ordinal comparison produce the same results.
/// </summary>
public class Unit
{
    public int Id { get; private set; }
    public int ProjectId { get; private set; }

    /// <summary>Floor number (1-based for structural units; 0 for legacy single-unit path).</summary>
    public int Floor { get; private set; }

    /// <summary>Room identifier within the floor (e.g. "01", "02").</summary>
    public string RoomNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Derived human-readable label: "{Floor}-{RoomNumber}" (§1.2).
    /// Stored, not computed on read — preserves legacy seed labels and simplifies query paths.
    /// </summary>
    public string UnitNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Owner email pre-assigned at structure-definition time (ADR-D — lower-cased).
    /// Null when no owner was pre-assigned. Used for email-based owner linking (§3.3).
    /// </summary>
    public string? OwnerEmail { get; private set; }

    /// <summary>
    /// DB-assigned owner identity. Nullable (D3): null until the owner-linking consumer
    /// or the unit-first inline match sets it (§3.3 / REQ-PS-02).
    /// </summary>
    public int? OwnerId { get; private set; }

    /// <summary>EF Core parameterless constructor — required for entity materialisation.</summary>
    protected Unit() { }

    /// <summary>
    /// Structure-definition constructor — used by DefineProjectStructureCommand (§1.3).
    /// OwnerId is always null at creation; email is lower-cased (ADR-D).
    /// </summary>
    public Unit(int projectId, int floor, string roomNumber, string? ownerEmail)
    {
        ProjectId = projectId;
        Floor = floor;
        RoomNumber = roomNumber;
        UnitNumber = ComposeUnitNumber(floor, roomNumber);
        OwnerEmail = ownerEmail?.ToLowerInvariant();
        OwnerId = null;
    }

    /// <summary>
    /// Legacy constructor — kept for backward compatibility with CreateUnitCommand callers (§1.3).
    /// Sets Floor = 0 and RoomNumber = unitNumber so existing code compiles unchanged.
    /// Mark as LEGACY; prefer the structure-definition ctor for new unit creation.
    /// </summary>
    public Unit(int projectId, string unitNumber, int ownerId)
    {
        ProjectId = projectId;
        Floor = 0;
        RoomNumber = unitNumber;
        UnitNumber = unitNumber;
        OwnerId = ownerId;
        OwnerEmail = null;
    }

    // ------------------------------------------------------------------ //
    //  Domain methods
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Links an owner by their IAM UserId (called by the owner-linking consumer, §3.2,
    /// and by the inline unit-first match in ProjectStructureCommandService, §3.3).
    /// </summary>
    public void LinkOwner(int ownerId) => OwnerId = ownerId;

    /// <summary>
    /// Assigns (or clears) the pre-assigned owner email, lower-casing the value (ADR-D).
    /// Called when an email pre-assignment is edited after initial structure definition.
    /// </summary>
    public void AssignOwnerEmail(string? email) =>
        OwnerEmail = email?.ToLowerInvariant();

    /// <summary>
    /// Derives the UnitNumber label from floor and room number (§1.2 / ADR-F).
    /// Example: floor 5, room "02" → "5-02".
    /// </summary>
    public static string ComposeUnitNumber(int floor, string room) => $"{floor}-{room}";
}
