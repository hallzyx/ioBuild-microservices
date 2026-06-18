namespace IoBuild.Projects.Domain.Services.Commands.Projects;

/// <summary>
/// Individual room/unit assignment within a floor (§1.3 design.md).
/// OwnerEmail is optional — set at structure-definition time when the builder
/// knows the assigned owner's email; null otherwise (linked later by the consumer).
/// </summary>
public record RoomSpec(string RoomNumber, string? OwnerEmail);

/// <summary>
/// Specification for a single floor in the project (§1.3 design.md).
/// Rooms is the ordered list of room specs for this floor.
/// </summary>
public record FloorSpec(int Floor, IReadOnlyList<RoomSpec> Rooms);

/// <summary>
/// Command to define the physical floor/room structure of a project in a single operation
/// (REQ-PS-01 / §1.3 design.md).
///
/// The REST layer expands a uniform "floors × unitsPerFloor" request into explicit
/// FloorSpec + RoomSpec lists before reaching this command, keeping the domain handler
/// single-path regardless of the UX shape used at the API boundary.
/// </summary>
public record DefineProjectStructureCommand(
    int ProjectId,
    IReadOnlyList<FloorSpec> Floors,
    int BuilderId);
