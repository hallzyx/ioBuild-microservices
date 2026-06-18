using System.Text.Json;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Persistence;

/// <summary>
/// One-time startup reconciliation that emits Created events for aggregates which
/// were persisted out-of-band. The EF <c>HasData</c> seed writes rows directly and
/// bypasses the command services (and therefore the outbox), so the Analytics read
/// model never learns about seeded data. This backfill enqueues the missing outbox
/// messages so the existing OutboxWorker publishes them through the normal pipeline.
///
/// Idempotency: only runs when the outbox is empty, so it never duplicates events on
/// a restart that already has outbox history.
/// </summary>
public static class OutboxBackfill
{
    public static async Task RunAsync(AppDbContext db)
    {
        // Skip if the outbox already has history (normal restart) — avoids duplicates.
        if (await db.OutboxMessages.AnyAsync())
            return;

        var projects = await db.Projects.AsNoTracking().ToListAsync();
        var units = await db.Units.AsNoTracking().ToListAsync();

        if (projects.Count == 0 && units.Count == 0)
            return;

        var builderByProject = projects.ToDictionary(p => p.Id, p => p.BuilderId);

        foreach (var p in projects)
        {
            var evt = new ProjectCreatedEvent
            {
                ProjectId = p.Id,
                BuilderUserId = p.BuilderId,
                Name = p.Name,
                Status = p.Status.ToString()
            };
            db.OutboxMessages.Add(new OutboxMessage(nameof(ProjectCreatedEvent), JsonSerializer.Serialize(evt))
            {
                EventId = evt.EventId
            });
        }

        foreach (var u in units)
        {
            var evt = new UnitCreatedEvent
            {
                UnitId = u.Id,
                ProjectId = u.ProjectId,
                BuilderUserId = builderByProject.GetValueOrDefault(u.ProjectId, 0),
                OwnerUserId = u.OwnerId,
                Status = "Active" // mirrors UnitCommandService
            };
            db.OutboxMessages.Add(new OutboxMessage(nameof(UnitCreatedEvent), JsonSerializer.Serialize(evt))
            {
                EventId = evt.EventId
            });
        }

        await db.SaveChangesAsync();
    }
}
