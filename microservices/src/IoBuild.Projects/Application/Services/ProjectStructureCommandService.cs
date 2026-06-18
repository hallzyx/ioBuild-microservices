using System.Text.Json;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Infrastructure.Persistence;
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
        }

        // ── Guard: 409 if project already has units (REQ-PS-03) ─────────
        var existingUnitCount = _dbContext.Units.Count(u => u.ProjectId == command.ProjectId);
        if (existingUnitCount > 0)
            throw new InvalidOperationException(
                $"Project {command.ProjectId} already has units defined. " +
                "Structure re-definition is not allowed (REQ-PS-03 / 409 Conflict).");

        // ── Phase 1: create all Unit rows ───────────────────────────────
        var createdUnits = new List<Unit>();
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
                Status = "Active"
            };
            outboxMessages.Add(new OutboxMessage(nameof(UnitCreatedEvent), JsonSerializer.Serialize(unitEvt))
            {
                EventId = unitEvt.EventId
            });
        }

        // One FloorStructureDefinedEvent per distinct floor (REQ-PS-05 / §6.1)
        foreach (var floorSpec in command.Floors)
        {
            var floorEvt = new FloorStructureDefinedEvent
            {
                ProjectId = command.ProjectId,
                Floor = floorSpec.Floor,
                UnitCount = floorSpec.Rooms.Count,
                BuilderId = command.BuilderId
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

        await _unitOfWork.CompleteAsync(); // Phase 2 — outbox + owner links committed
    }
}
