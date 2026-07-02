using System.Text.Json;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Infrastructure.Persistence;
using IoBuild.Shared.Domain.Model;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Application.Services;

/// <summary>
/// Handles <see cref="DefineProjectStructureCommand"/> — creates all units for a project
/// in a single two-phase commit and emits the required outbox events (§1.3 / ADR-A).
///
/// Two-phase commit rationale (ADR-A):
///   Phase 1 — persist all Unit rows so the DB assigns real auto-increment Ids.
///   Phase 2 — build UnitCreatedEvent / FloorStructureDefinedEvent / UnitOwnerMatchedEvent
///              with those real Ids, then persist outbox rows in a second CompleteAsync.
///   A crash between the two commits leaves units without events; the existing
///   idempotent OutboxBackfill re-emits them on next startup (at-least-once delivery).
/// </summary>
public class ProjectStructureCommandService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IOutboxMessageRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _dbContext;

    public ProjectStructureCommandService(
        IUnitRepository unitRepository,
        IProjectRepository projectRepository,
        IOutboxMessageRepository outboxRepository,
        AppDbContext dbContext)
    {
        _unitRepository = unitRepository;
        _projectRepository = projectRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = dbContext;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Defines the project structure: validates inputs, guards for already-defined projects,
    /// creates units with two-phase commit, and emits the required outbox events.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when floors or rooms per floor &lt; 1 (HTTP 422).</exception>
    /// <exception cref="InvalidOperationException">Thrown when project already has units (HTTP 409).</exception>
    public async Task Handle(DefineProjectStructureCommand command)
    {
        // ── Validation (REQ-PS-01 / §1.3) ──────────────────────────────
        if (!command.Floors.Any() || command.Floors.Count < 1)
            throw new ArgumentException("floors must be at least 1.", nameof(command.Floors));

        foreach (var floorSpec in command.Floors)
        {
            if (floorSpec.Rooms == null || floorSpec.Rooms.Count < 1)
                throw new ArgumentException(
                    $"Floor {floorSpec.Floor} must have at least 1 room (unitsPerFloor ≥ 1).",
                    nameof(command.Floors));

            // ── Device-type validation (S2.3 / S2.4) ────────────────────
            // Reject unknown device type codes with ArgumentException → HTTP 422/400.
            if (floorSpec.DeviceTypes is { Count: > 0 })
            {
                var unknownTypes = floorSpec.DeviceTypes
                    .Where(t => !DeviceTypeCatalog.KnownTypes.Contains(t))
                    .Distinct()
                    .ToList();

                if (unknownTypes.Count > 0)
                    throw new ArgumentException(
                        $"Floor {floorSpec.Floor} contains unknown device type(s): " +
                        $"{string.Join(", ", unknownTypes)}. " +
                        $"Valid types are: {string.Join(", ", DeviceTypeCatalog.KnownTypes)}.",
                        nameof(command.Floors));
            }

            // ── Per-unit package validation (ADR-9 / T-11) ───────────────
            // Reject unknown type codes and duplicate types within a single unit.
            foreach (var roomSpec in floorSpec.Rooms)
            {
                if (roomSpec.DeviceTypes is not { Count: > 0 })
                    continue;

                // Reject unknown types
                var unknownUnitTypes = roomSpec.DeviceTypes
                    .Where(t => !DeviceTypeCatalog.KnownTypes.Contains(t))
                    .Distinct()
                    .ToList();

                if (unknownUnitTypes.Count > 0)
                    throw new ArgumentException(
                        $"Floor {floorSpec.Floor} room {roomSpec.RoomNumber} contains unknown " +
                        $"device type(s) in unit package: {string.Join(", ", unknownUnitTypes)}. " +
                        $"Valid types are: {string.Join(", ", DeviceTypeCatalog.KnownTypes)}.",
                        nameof(command.Floors));

                // Reject duplicate types within the same unit package (spec: C5 / edge-case register)
                var duplicates = roomSpec.DeviceTypes
                    .GroupBy(t => t, StringComparer.Ordinal)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Count > 0)
                    throw new ArgumentException(
                        $"Floor {floorSpec.Floor} room {roomSpec.RoomNumber} has duplicate " +
                        $"device type(s) in unit package: {string.Join(", ", duplicates)}. " +
                        "Each type may appear at most once per unit.",
                        nameof(command.Floors));
            }
        }

        // ── Guard: 409 if project already has units (REQ-PS-03) ─────────
        var existingUnitCount = _dbContext.Units.Count(u => u.ProjectId == command.ProjectId);
        if (existingUnitCount > 0)
            throw new InvalidOperationException(
                $"Project {command.ProjectId} already has units defined. " +
                "Structure re-definition is not allowed (REQ-PS-03 / 409 Conflict).");

        // ── Phase 1: create all Unit rows ───────────────────────────────
        // Track (Unit, RoomSpec) pairs so Phase 2 can access RoomSpec.DeviceTypes for
        // per-unit package events without a second lookup (ADR-9 / T-11).
        var createdUnits = new List<Unit>();
        var unitRoomSpecPairs = new List<(Unit Unit, RoomSpec Room, int Floor)>();

        foreach (var floorSpec in command.Floors)
        {
            foreach (var roomSpec in floorSpec.Rooms)
            {
                var unit = new Unit(
                    projectId: command.ProjectId,
                    floor: floorSpec.Floor,
                    roomNumber: roomSpec.RoomNumber,
                    ownerEmail: roomSpec.OwnerEmail);

                await _unitRepository.AddAsync(unit);
                createdUnits.Add(unit);
                unitRoomSpecPairs.Add((unit, roomSpec, floorSpec.Floor));
            }
        }

        // First CompleteAsync — DB assigns real Ids to all units (ADR-A)
        await _unitOfWork.CompleteAsync();

        // ── Phase 2: build and persist outbox rows ──────────────────────
        var outboxMessages = new List<OutboxMessage>();

        // One UnitCreatedEvent per unit (with real UnitId, §1.3 / REQ-PS-04)
        foreach (var unit in createdUnits)
        {
            var unitEvt = new UnitCreatedEvent
            {
                UnitId = unit.Id,               // real DB id (ADR-A)
                ProjectId = unit.ProjectId,
                BuilderUserId = command.BuilderId,
                OwnerUserId = null,
                Floor = unit.Floor,
                RoomNumber = unit.RoomNumber,
                OwnerEmail = unit.OwnerEmail,
                Status = !string.IsNullOrEmpty(unit.OwnerEmail) ? "Occupied" : "Active"
            };
            outboxMessages.Add(new OutboxMessage(nameof(UnitCreatedEvent), JsonSerializer.Serialize(unitEvt))
            {
                EventId = unitEvt.EventId
            });
        }

        // One UnitDevicesDefinedEvent per unit that has a non-empty device package (ADR-9 / T-11)
        // Package types were already validated above (unknown codes and duplicates rejected).
        foreach (var (unit, roomSpec, floor) in unitRoomSpecPairs)
        {
            // Skip units with no package or empty package
            if (roomSpec.DeviceTypes is not { Count: > 0 })
                continue;

            // Types are already validated (no unknowns, no duplicates) — emit directly
            var unitPkgEvt = new UnitDevicesDefinedEvent
            {
                UnitId = unit.Id,               // real Phase-1 id (ADR-9)
                ProjectId = command.ProjectId,
                Floor = floor,
                RoomNumber = roomSpec.RoomNumber,
                BuilderId = command.BuilderId,
                DeviceTypes = roomSpec.DeviceTypes.ToList()
            };
            outboxMessages.Add(new OutboxMessage(nameof(UnitDevicesDefinedEvent), JsonSerializer.Serialize(unitPkgEvt))
            {
                EventId = unitPkgEvt.EventId
            });
        }

        // One FloorStructureDefinedEvent per distinct floor (REQ-PS-05 / §6.1)
        foreach (var floorSpec in command.Floors)
        {
            // Resolve per-floor device types: deduplicate, normalize empty → null (S2.3 / S2.5 / S2.6)
            IReadOnlyList<string>? resolvedDeviceTypes = null;
            if (floorSpec.DeviceTypes is { Count: > 0 })
            {
                var deduped = floorSpec.DeviceTypes.Distinct().ToList();
                resolvedDeviceTypes = deduped.Count > 0 ? deduped : null;
            }

            var floorEvt = new FloorStructureDefinedEvent
            {
                ProjectId = command.ProjectId,
                Floor = floorSpec.Floor,
                UnitCount = floorSpec.Rooms.Count,
                BuilderId = command.BuilderId,
                DeviceTypes = resolvedDeviceTypes
            };
            outboxMessages.Add(new OutboxMessage(nameof(FloorStructureDefinedEvent), JsonSerializer.Serialize(floorEvt))
            {
                EventId = floorEvt.EventId
            });
        }

        // ── Unit-first owner match (§3.3 / ADR-B) ──────────────────────
        // For each unit with an assigned OwnerEmail, look up the RegisteredOwner mirror.
        // If found and OwnerId is still null, set it immediately and emit UnitOwnerMatchedEvent.
        // This covers the case where the owner registered BEFORE the structure was defined.
        var unitsWithEmail = createdUnits
            .Where(u => !string.IsNullOrEmpty(u.OwnerEmail))
            .ToList();

        if (unitsWithEmail.Count > 0)
        {
            var emailsToCheck = unitsWithEmail
                .Select(u => u.OwnerEmail!)
                .Distinct()
                .ToList();

            var registeredOwners = await _dbContext.RegisteredOwners
                .Where(ro => emailsToCheck.Contains(ro.Email))
                .ToDictionaryAsync(ro => ro.Email, ro => ro.UserId);

            foreach (var unit in unitsWithEmail)
            {
                if (unit.OwnerId.HasValue)
                    continue; // already linked (safety guard)

                if (!registeredOwners.TryGetValue(unit.OwnerEmail!, out var ownerId))
                    continue; // no match — owner hasn't registered yet (registration-first path)

                unit.LinkOwner(ownerId);

                var matchEvt = new UnitOwnerMatchedEvent
                {
                    UnitId = unit.Id,
                    ProjectId = unit.ProjectId,
                    OwnerUserId = ownerId,
                    OwnerEmail = unit.OwnerEmail!
                };
                outboxMessages.Add(new OutboxMessage(nameof(UnitOwnerMatchedEvent), JsonSerializer.Serialize(matchEvt))
                {
                    EventId = matchEvt.EventId
                });
            }
        }

        // Persist all outbox rows (and any OwnerId mutations) in one second transaction
        foreach (var msg in outboxMessages)
            await _outboxRepository.AddAsync(msg);

        // ── Project totals sync (B2) ────────────────────────────────────
        // Recompute TotalUnits and OccupiedUnits from the created units.
        // OccupiedUnits = count of units where OwnerEmail != null (not OwnerId —
        // owner linking is async; a pre-assigned email IS occupied regardless of OwnerId state).
        var project = await _projectRepository.FindByIdAsync(command.ProjectId);
        if (project != null)
        {
            var totalUnits = createdUnits.Count;
            var occupiedUnits = createdUnits.Count(u => u.OwnerEmail != null);
            project.Update(
                project.Name,
                project.Description,
                project.Location,
                totalUnits,
                occupiedUnits,
                project.Status,
                project.ImageUrl);
            _projectRepository.Update(project);
        }

        await _unitOfWork.CompleteAsync(); // Phase 2 — outbox + owner links + totals committed
    }
}
