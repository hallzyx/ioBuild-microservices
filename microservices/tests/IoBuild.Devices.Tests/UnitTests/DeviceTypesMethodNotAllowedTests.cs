using System.Reflection;
using FluentAssertions;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Interfaces.REST;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IoBuild.Devices.Tests.UnitTests;

/// <summary>
/// SC-1.3: POST /api/v1/devices/types must return HTTP 405 Method Not Allowed.
/// ASP.NET MVC returns 405 for any HTTP method that has no matching [HttpXxx] action
/// on a route that is otherwise known to the router. This test exercises the actual
/// routing layer using a minimal in-process TestServer.
///
/// Infrastructure notes:
/// - AddControllers().AddApplicationPart() is required because the test runner's
///   entry assembly differs from IoBuild.Devices.dll.
/// - ITelemetryQueryService must be registered (TelemetryController is in the same
///   assembly and needs it; missing it causes controller discovery to fail).
/// - The custom [Authorize] filter fires for GET (returns 401) but 405 is decided
///   at the routing layer before filters run, so POST → 405 is correct behavior.
///
/// TDD: T-W1 RED → GREEN. Red: route not found (404) until ApplicationPart registered.
/// Green: route matches → ASP.NET routing signals 405 for unmapped POST.
/// </summary>
public class DeviceTypesMethodNotAllowedTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        // Minimal MVC with the same routing convention used in production.
        // AddApplicationPart is required: the test runner assembly is not IoBuild.Devices.dll,
        // so AddControllers() would not discover DevicesController or TelemetryController
        // without this explicit registration.
        builder.Services
            .AddControllers(options =>
            {
                options.Conventions.Add(new KebabCaseRouteNamingConvention());
            })
            .AddApplicationPart(typeof(DevicesController).Assembly);

        // Register mocks for all controller constructor dependencies in the Devices assembly.
        // DevicesController   → IDeviceCommandService, IDeviceQueryService
        // TelemetryController → IDeviceQueryService,   ITelemetryQueryService
        builder.Services.AddSingleton(new Mock<IDeviceCommandService>().Object);
        builder.Services.AddSingleton(new Mock<IDeviceQueryService>().Object);
        builder.Services.AddSingleton(new Mock<ITelemetryQueryService>().Object);

        _app = builder.Build();
        _app.UseRouting();
        _app.MapControllers();

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    /// <summary>
    /// SC-1.3: POST to /api/v1/devices/types must return 405 Method Not Allowed.
    /// The route template api/v1/devices/types is registered (GET is defined on it),
    /// so ASP.NET routing signals 405 for any method with no matching action.
    /// </summary>
    [Fact]
    public async Task PostDeviceTypes_Returns405_MethodNotAllowed()
    {
        var response = await _client.PostAsync("/api/v1/devices/types", content: null);

        response.StatusCode.Should().Be(
            System.Net.HttpStatusCode.MethodNotAllowed,
            "POST is not mapped on the GET-only /api/v1/devices/types route (SC-1.3)");
    }
}
