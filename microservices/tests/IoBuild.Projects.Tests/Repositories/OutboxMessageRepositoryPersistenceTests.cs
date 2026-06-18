using FluentAssertions;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Infrastructure.Persistence;
using IoBuild.Projects.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Tests.Repositories;

/// <summary>
/// Persistence test for CRITICAL-2 (verify-report.md — WARNING-B coverage gap):
/// OutboxMessageRepository.UpdateAsync must persist the status change to the DB.
/// Mirrors IoBuild.Devices.Tests.Repositories.OutboxMessageRepositoryPersistenceTests
/// using the three-context pattern (write / worker / read) to verify real EF persistence.
/// </summary>
public class OutboxMessageRepositoryPersistenceTests
{
    private static AppDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task UpdateAsync_PersistsStatusChange_ToDatabase()
    {
        // Arrange — seed a Pending outbox row using a fresh context (simulates the write scope)
        var dbName = nameof(UpdateAsync_PersistsStatusChange_ToDatabase);

        await using (var writeCtx = BuildContext(dbName))
        {
            var msg = new OutboxMessage("ProjectCreatedEvent", """{"ProjectId":1}""")
            {
                EventId = Guid.NewGuid()
            };
            await writeCtx.OutboxMessages.AddAsync(msg);
            await writeCtx.SaveChangesAsync();
        }

        // Act — simulate the worker scope: open new context, call UpdateAsync, dispose scope
        await using (var workerCtx = BuildContext(dbName))
        {
            var repo = new OutboxMessageRepository(workerCtx);

            var pending = await workerCtx.OutboxMessages
                .FirstAsync(m => m.Status == "Pending");

            // Mark processed (mirrors what OutboxWorker does after successful publish)
            pending.Status = "Processed";
            pending.ProcessedAt = DateTime.UtcNow;

            await repo.UpdateAsync(pending);
            // Scope disposes here — if SaveChangesAsync was not called, mutation is lost
        }

        // Assert — open a THIRD context to verify persistence (simulates re-read after scope disposal)
        await using (var readCtx = BuildContext(dbName))
        {
            var row = await readCtx.OutboxMessages.FirstAsync();
            row.Status.Should().Be("Processed",
                "UpdateAsync must call SaveChangesAsync so the status is persisted when the scope disposes");
            row.ProcessedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetPendingAsync_DoesNotReturnProcessedRows()
    {
        // Regression guard: once UpdateAsync saves correctly, GetPendingAsync must not return Processed rows
        var dbName = nameof(GetPendingAsync_DoesNotReturnProcessedRows);

        await using (var writeCtx = BuildContext(dbName))
        {
            var msg = new OutboxMessage("ProjectUpdatedEvent", """{"ProjectId":2}""")
            {
                EventId = Guid.NewGuid(),
                Status = "Processed",
                ProcessedAt = DateTime.UtcNow
            };
            await writeCtx.OutboxMessages.AddAsync(msg);
            await writeCtx.SaveChangesAsync();
        }

        await using (var readCtx = BuildContext(dbName))
        {
            var repo = new OutboxMessageRepository(readCtx);
            var pending = await repo.GetPendingAsync();
            pending.Should().BeEmpty("processed rows must not be returned as pending");
        }
    }
}
