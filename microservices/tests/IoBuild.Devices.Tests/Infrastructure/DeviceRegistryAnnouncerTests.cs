using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Infrastructure.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace IoBuild.Devices.Tests.Infrastructure;

public class DeviceRegistryAnnouncerTests
{
    [Fact]
    public async Task AnnounceAllAsync_PublishesRetainedRegistryForEveryDevice()
    {
        // Arrange
        var repo = new Mock<IDeviceRepository>();
        repo.Setup(r => r.ListAsync()).ReturnsAsync(new List<Device>
        {
            new("AirConditioner #13", "AirConditioner", "Sector-A", "AA:13", 1, "Active"),
            new("SmartLight #16", "SmartLight", "Sector-B", "AA:16", 1, "Active"),
        });

        var services = new ServiceCollection();
        services.AddScoped(_ => repo.Object);
        var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        var mqtt = new Mock<IMqttPublisher>();
        var logger = new Mock<ILogger<DeviceRegistryAnnouncer>>().Object;
        var announcer = new DeviceRegistryAnnouncer(scopeFactory, mqtt.Object, logger);

        // Act
        await announcer.AnnounceAllAsync(CancellationToken.None);

        // Assert — one retained registry publish per device, with correct type
        mqtt.Verify(m => m.EnqueueRawAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("\"type\":\"AirConditioner\"")),
            true, It.IsAny<CancellationToken>()), Times.Once);
        mqtt.Verify(m => m.EnqueueRawAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("\"type\":\"SmartLight\"")),
            true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
