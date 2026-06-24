using FluentAssertions;
using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Devices.Interfaces.REST;
using IoBuild.Devices.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IoBuild.Devices.Tests.UnitTests;

/// <summary>
/// Task 3.1 — GET /api/v1/devices/types must include ControllableAttributes populated
/// from the DB catalog. Telemetry-only types return empty list.
///
/// Updated for device-type-catalog Slice 1 (WU-1):
///   Source is now DB catalog seeded via HasData migration (design D4/D5).
///   Controller requires DeviceTypeCatalogQueryService (injected).
/// </summary>
public class DeviceTypeCatalogControllableAttributesTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceTypeCatalogControllableAttributesTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private DevicesDbContext BuildRelationalContext()
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseSqlite(_connection)
            .Options;
        var ctx = new DevicesDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static DevicesController BuildController(DevicesDbContext db)
    {
        var cmdSvc = new Mock<IoBuild.Devices.Domain.Services.IDeviceCommandService>().Object;
        var querySvc = new DeviceQueryService(new DeviceRepository(db));
        var catalogSvc = new DeviceTypeCatalogQueryService(new DeviceTypeRepository(db));
        return new DevicesController(cmdSvc, querySvc, catalogSvc);
    }

    [Fact]
    public async Task GetDeviceTypes_Returns_ControllableAttributes_ForAcAndSmartLight()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();
        var result = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = result.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;

        // AirConditioner must have controllable attributes from DB catalog
        var acEntry = body.DeviceTypes.SingleOrDefault(t => t.Code == "AirConditioner");
        acEntry.Should().NotBeNull("AirConditioner must be in the catalog");
        acEntry!.ControllableAttributes.Should().NotBeEmpty(
            "AirConditioner has targetTemperature, mode, power in the DB catalog");

        var tempAttr = acEntry.ControllableAttributes.SingleOrDefault(a => a.Name == "targetTemperature");
        tempAttr.Should().NotBeNull("targetTemperature must be declared for AirConditioner");
        tempAttr!.Type.Should().Be("number");
        tempAttr.Min.Should().Be(16);
        tempAttr.Max.Should().Be(30);
        tempAttr.Unit.Should().Be("C");

        // SmartLight must have controllable attributes from DB catalog
        var slEntry = body.DeviceTypes.SingleOrDefault(t => t.Code == "SmartLight");
        slEntry.Should().NotBeNull("SmartLight must be in the catalog");
        slEntry!.ControllableAttributes.Should().NotBeEmpty(
            "SmartLight has brightness and power in the DB catalog");

        var brightnessAttr = slEntry.ControllableAttributes.SingleOrDefault(a => a.Name == "brightness");
        brightnessAttr.Should().NotBeNull("brightness must be declared for SmartLight");
        brightnessAttr!.Min.Should().Be(0);
        brightnessAttr.Max.Should().Be(100);
    }

    [Fact]
    public async Task GetDeviceTypes_TelemetryOnlyTypes_HaveEmptyControllableAttributes()
    {
        await using var db = BuildRelationalContext();
        var controller = BuildController(db);

        var actionResult = await controller.GetDeviceTypes();
        var result = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var body = result.Value.Should().BeOfType<DeviceTypeCatalogResource>().Subject;

        // Telemetry-only types must have empty ControllableAttributes
        var telemetryTypes = new[] { "SmartMeter", "WaterSensor", "SmokeDetector" };
        foreach (var type in telemetryTypes)
        {
            var entry = body.DeviceTypes.SingleOrDefault(t => t.Code == type);
            entry.Should().NotBeNull($"{type} must be in the catalog");
            entry!.ControllableAttributes.Should().BeEmpty(
                $"{type} is telemetry-only; ControllableAttributes must be empty");
        }
    }
}
