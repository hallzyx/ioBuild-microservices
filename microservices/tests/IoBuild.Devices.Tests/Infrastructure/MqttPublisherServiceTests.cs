using System.Threading.Channels;
using FluentAssertions;
using IoBuild.Devices.Infrastructure.Mqtt;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Protocol;

namespace IoBuild.Devices.Tests.Infrastructure;

/// <summary>
/// TDD RED tests for tasks 1.1 and 1.2 — MqttPublisherService channel drain + reconnect.
///
/// Test seam: internal constructor accepts a fake IMqttClient so no real broker is needed.
/// We drain the channel synchronously via a bounded channel + short-lived cancellation token.
/// </summary>
public class MqttPublisherServiceTests
{
    // ── MP-01: EnqueueAsync → channel → publish called with correct topic + QoS1 + retain ──

    [Fact]
    public async Task MqttPublisherService_Enqueue_DrainsThroughChannel_CallsClientPublish()
    {
        // Arrange
        var fakeClient = new Mock<IMqttClient>();
        fakeClient
            .Setup(c => c.IsConnected)
            .Returns(true);
        fakeClient
            .Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(0, MqttClientPublishReasonCode.Success, string.Empty, null));

        var logger = new Mock<ILogger<MqttPublisherService>>().Object;
        using var cts = new CancellationTokenSource();

        var svc = new MqttPublisherService(fakeClient.Object, logger);

        // Act — enqueue one command, run drain for a short window, then cancel
        await svc.EnqueueAsync("42", """{"targetTemperature":22}""");

        // Give the background loop a moment to drain
        await Task.Delay(200);
        cts.Cancel();

        // Assert — publish was called with the correct topic
        fakeClient.Verify(c => c.PublishAsync(
            It.Is<MqttApplicationMessage>(m =>
                m.Topic == "commands/42" &&
                m.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtLeastOnce &&
                m.Retain == true),
            It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    // ── MP-02: Reconnect — service re-establishes connection after simulated disconnect ──

    [Fact]
    public async Task MqttPublisherService_Reconnects_WhenClientDrops()
    {
        // Arrange — client starts disconnected, then becomes "connected" after reconnect call
        var callCount = 0;
        var fakeClient = new Mock<IMqttClient>();
        fakeClient
            .SetupSequence(c => c.IsConnected)
            .Returns(false)   // first check → disconnected → triggers reconnect
            .Returns(true)    // after reconnect attempt → connected
            .Returns(true);

        fakeClient
            .Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult())
            .Callback(() => callCount++);

        fakeClient
            .Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(0, MqttClientPublishReasonCode.Success, string.Empty, null));

        var logger = new Mock<ILogger<MqttPublisherService>>().Object;
        var svc = new MqttPublisherService(fakeClient.Object, logger);

        // Act
        await svc.EnqueueAsync("7", """{"brightness":80}""");
        await Task.Delay(300);

        // Assert — ConnectAsync was called at least once (reconnect attempted)
        callCount.Should().BeGreaterThanOrEqualTo(1, "service must reconnect when IsConnected is false");
    }
}
