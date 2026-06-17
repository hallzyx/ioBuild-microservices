using FluentAssertions;
using IoBuild.Analytics.Application.Internal.QueryServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IoBuild.Analytics.Tests.Application;

/// <summary>
/// REQ-AQ-01, AQ-S03 — AnalyticsQueryService must no longer depend on
/// IDevicesContextFacade or IProjectsContextFacade.
/// Verified structurally via constructor parameter inspection.
/// </summary>
public class NoHttpCallQueryTests
{
    [Fact]
    public void AnalyticsQueryService_constructor_does_not_accept_facade_dependencies()
    {
        // If the constructor still takes IDevicesContextFacade or IProjectsContextFacade,
        // this test fails — proving the HTTP calls have been removed.
        var constructors = typeof(AnalyticsQueryService).GetConstructors();
        var parameterTypes = constructors
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType.Name)
            .ToList();

        parameterTypes.Should().NotContain("IDevicesContextFacade",
            "AnalyticsQueryService must not depend on the devices HTTP facade");
        parameterTypes.Should().NotContain("IProjectsContextFacade",
            "AnalyticsQueryService must not depend on the projects HTTP facade");
    }

    [Fact]
    public void AnalyticsQueryService_can_be_constructed_with_only_db_and_logger()
    {
        // Proves the new minimal constructor works without any HTTP client dependencies
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(nameof(AnalyticsQueryService_can_be_constructed_with_only_db_and_logger))
            .Options;
        using var db = new AnalyticsDbContext(options);

        var act = () => new AnalyticsQueryService(db, NullLogger<AnalyticsQueryService>.Instance);
        act.Should().NotThrow();
    }
}
