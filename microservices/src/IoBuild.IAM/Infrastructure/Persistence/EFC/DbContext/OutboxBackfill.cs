using System.Text.Json;
using IoBuild.IAM.Domain.Model.Entities;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.IAM.Infrastructure.Persistence.EFC.DbContext;

/// <summary>
/// One-time startup reconciliation that emits UserRegisteredEvent for users which
/// were persisted out-of-band (HasData seed bypasses command services and the outbox).
/// This ensures the Projects owner-linking consumer learns about seeded IAM users so
/// the demo owner→unit linking flow works on first boot.
///
/// Idempotency: only runs when the outbox is empty — avoids duplicates on normal restart.
/// Mirrors IoBuild.Projects.Infrastructure.Persistence.OutboxBackfill.
/// </summary>
public static class OutboxBackfill
{
    public static async Task RunAsync(ApplicationDbContext db)
    {
        // Skip if the outbox already has history (normal restart) — avoids duplicates.
        if (await db.OutboxMessages.AnyAsync())
            return;

        var users = await db.Users.AsNoTracking().ToListAsync();
        if (users.Count == 0)
            return;

        foreach (var u in users)
        {
            var evt = new UserRegisteredEvent
            {
                UserId = u.Id,
                Email = u.Email.ToLowerInvariant(), // ADR-D: always lower-case
                Role = u.Role
            };
            db.OutboxMessages.Add(new OutboxMessage(nameof(UserRegisteredEvent),
                JsonSerializer.Serialize(evt))
            {
                EventId = evt.EventId
            });
        }

        await db.SaveChangesAsync();
    }
}
