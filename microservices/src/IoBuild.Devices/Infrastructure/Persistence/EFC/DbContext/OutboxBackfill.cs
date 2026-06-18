using System.Text.Json;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;

/// <summary>
/// One-time startup reconciliation that emits DeviceCreated events for devices which
/// were persisted out-of-band. The EF <c>HasData</c> seed writes rows directly and
/// bypasses the command services (and therefore the outbox), so the Analytics read
/// model never learns about seeded devices. This backfill enqueues the missing outbox
/// messages so the existing OutboxWorker publishes them through the normal pipeline.
///
/// Idempotency: only runs when the outbox is empty, so it never duplicates events on
/// a restart that already has outbox history.
/// </summary>
public static class OutboxBackfill
{
    public static async Task RunAsync(DevicesDbContext db)
    {
        // Skip if the outbox already has history (normal restart) — avoids duplicates.
        if (await db.OutboxMessages.AnyAsync())
            return;

        var devices = await db.Devices.AsNoTracking().ToListAsync();
        if (devices.Count == 0)
            return;

        foreach (var d in devices)
        {
            var evt = new DeviceCreatedEvent
            {
                DeviceId = d.Id,
                OwnerUserId = 0, // mirrors DeviceCommandService (Device aggregate has no owner yet)
                ProjectId = d.ProjectId,
                DeviceType = d.Type,
                Status = d.Status
            };
            db.OutboxMessages.Add(new OutboxMessage(nameof(DeviceCreatedEvent), JsonSerializer.Serialize(evt))
            {
                EventId = evt.EventId
            });
        }

        await db.SaveChangesAsync();
    }
}
