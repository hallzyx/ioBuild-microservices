using FluentAssertions;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Analytics.Infrastructure.Messaging;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace IoBuild.Analytics.Tests.Infrastructure;

/// <summary>
/// RED test for CRITICAL-1 (verify-report.md):
/// AnalyticsEventConsumer built with the PRODUCTION constructor must populate
/// the read model when ApplyEventByTypeAsync is invoked.
///
/// Without the fix, _directDb is null and GetDb() throws InvalidOperationException
/// inside every upsert method — causing every delivery to be nacked and discarded.
///
/// This test exercises the production code path:
///   ProductionConstructor(IServiceScopeFactory) → ApplyEventByTypeAsync → upsert
/// It must FAIL against the current code where upserts call GetDb() which throws
/// when _directDb is null.
/// </summary>
public class ProductionConsumerPathTests
{
    /// <summary>
    /// Builds a ServiceProvider that wires up AnalyticsDbContext on EF InMemory,
    /// exactly as a real DI container would in tests that use the production constructor.
    /// </summary>
    private static (IServiceProvider provider, string dbName) BuildProductionServices()
    {
        var dbName = $"analytics-prod-{Guid.NewGuid()}";

        var services = new ServiceCollection();

        services.AddDbContext<AnalyticsDbContext>(opt =>
            opt.UseInMemoryDatabase(dbName));

        // Minimal IConfiguration so the production constructor does not fail on null
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Intentionally empty — no real broker needed; we won't call ExecuteAsync
                ["RabbitMq:ConnectionString"] = null
            })
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton(NullLogger<AnalyticsEventConsumer>.Instance);

        return (services.BuildServiceProvider(), dbName);
    }

    [Fact]
    public async Task ProductionConstructor_DeviceCreatedEvent_PopulatesDeviceProjection()
    {
        // Arrange — build the consumer using the PRODUCTION constructor (scopeFactory path)
        var (provider, _) = BuildProductionServices();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var config = provider.GetRequiredService<IConfiguration>();
        var logger = NullLogger<AnalyticsEventConsumer>.Instance;

        // This is the PRODUCTION constructor — _directDb stays null
        var consumer = new AnalyticsEventConsumer(scopeFactory, config, logger);

        var evt = new DeviceCreatedEvent
        {
            DeviceId = 99,
            OwnerUserId = 0,
            ProjectId = 5,
            UnitId = null,
            DeviceType = "Temperature",
            Status = "Online"
        };

        // Act — call ApplyEventAsync through the production constructor path
        // With the bug: _directDb is null → GetDb() throws → row never written
        // After fix: scope is opened → AnalyticsDbContext resolved → row is written
        await consumer.ApplyEventAsync(evt);

        // Assert — the projection row must exist in the database
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        var rows = await db.DeviceProjections.ToListAsync();

        rows.Should().HaveCount(1, "the production consumer path must write a DeviceProjection row");
        rows[0].DeviceId.Should().Be(99);
        rows[0].DeviceType.Should().Be("Temperature");
        rows[0].Status.Should().Be("Online");
    }

    [Fact]
    public async Task ProductionConstructor_ProjectCreatedEvent_PopulatesProjectProjection()
    {
        // Arrange
        var (provider, _) = BuildProductionServices();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var config = provider.GetRequiredService<IConfiguration>();
        var logger = NullLogger<AnalyticsEventConsumer>.Instance;

        var consumer = new AnalyticsEventConsumer(scopeFactory, config, logger);

        var evt = new ProjectCreatedEvent
        {
            ProjectId = 7,
            BuilderUserId = 3,
            Name = "Smart Home",
            Status = "Active"
        };

        // Act
        await consumer.ApplyEventAsync(evt);

        // Assert
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        var rows = await db.ProjectProjections.ToListAsync();

        rows.Should().HaveCount(1, "the production consumer path must write a ProjectProjection row");
        rows[0].ProjectId.Should().Be(7);
        rows[0].BuilderUserId.Should().Be(3);
        rows[0].Name.Should().Be("Smart Home");
    }
}
