using FluentAssertions;
using IoBuild.Devices.Domain.Model.Entities;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Devices.Tests.Repositories;

/// <summary>
/// RED test for CRITICAL-2 (verify-report.md):
/// OutboxMessageRepository.UpdateAsync must persist the status change to the DB.
/// Without SaveChangesAsync the mutation is lost when the scope disposes.
///
/// This test exercises the REAL production code path (no mocks for the repository).
/// It will FAIL against the current implementation that calls Update() without SaveChangesAsync.
/// </summary>
public class OutboxMessageRepositoryPersistenceTests
{
    private static DevicesDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new DevicesDbContext(options);
    }

    [Fact]
    public async Task UpdateAsync_PersistsStatusChange_ToDatabase()
    {
        // Arrange — seed a Pending outbox row using a fresh context (simulates the write scope)
        var dbName = nameof(UpdateAsync_PersistsStatusChange_ToDatabase);

        await using (var writeCtx = BuildContext(dbName))
        {
            var msg = new OutboxMessage("DeviceCreatedEvent", """{"DeviceId":1}""")
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
            var msg = new OutboxMessage("DeviceCreatedEvent", """{"DeviceId":2}""")
            {
                EventId = Guid.NewGuid(),
                Status = "Processed",
                ProcessedAt = DateTime.UtcNow
            };
            // Force status via EF (bypass private setter pattern if any)
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
