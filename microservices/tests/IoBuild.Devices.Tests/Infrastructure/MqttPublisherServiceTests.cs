using FluentAssertions;
using IoBuild.Devices.Infrastructure.Mqtt;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Protocol;

namespace IoBuild.Devices.Tests.Infrastructure;

/// <summary>
/// TDD tests for MqttPublisherService channel drain + reconnect.
///
/// Test seam: internal constructor accepts a fake IMqttClient so no real broker is needed.
///
/// Timing: the service drains the channel on a background loop, so the publish/connect
/// call happens asynchronously after Enqueue returns. Tests wait on a
/// <see cref="TaskCompletionSource"/> signalled from the mock callback (with a generous
/// timeout) instead of a fixed Task.Delay — this is deterministic and does not flake
/// under parallel/CPU-starved test runs.
/// </summary>
public class MqttPublisherServiceTests
{
    private static readonly TimeSpan DrainTimeout = TimeSpan.FromSeconds(5);

    // ── MP-01: EnqueueAsync → channel → publish called with correct topic + QoS1 + retain ──

    [Fact]
    public async Task MqttPublisherService_Enqueue_DrainsThroughChannel_CallsClientPublish()
    {
        // Arrange
        var fakeClient = new Mock<IMqttClient>();
        fakeClient
            .Setup(c => c.IsConnected)
            .Returns(true);

        var published = new TaskCompletionSource();
        fakeClient
            .Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(0, MqttClientPublishReasonCode.Success, string.Empty, null))
            .Callback(() => published.TrySetResult());

        var logger = new Mock<ILogger<MqttPublisherService>>().Object;
        var svc = new MqttPublisherService(fakeClient.Object, logger);

        // Act — enqueue one command, then wait until the background loop actually publishes
        await svc.EnqueueAsync("42", """{"targetTemperature":22}""");
        await published.Task.WaitAsync(DrainTimeout);

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
        var reconnected = new TaskCompletionSource();
        var fakeClient = new Mock<IMqttClient>();
        fakeClient
            .SetupSequence(c => c.IsConnected)
            .Returns(false)   // first check → disconnected → triggers reconnect
            .Returns(true)    // after reconnect attempt → connected
            .Returns(true);

        fakeClient
            .Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult())
            .Callback(() =>
            {
                callCount++;
                reconnected.TrySetResult();
            });

        fakeClient
            .Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(0, MqttClientPublishReasonCode.Success, string.Empty, null));

        var logger = new Mock<ILogger<MqttPublisherService>>().Object;
        var svc = new MqttPublisherService(fakeClient.Object, logger);

        // Act — enqueue, then wait until the background loop attempts to (re)connect
        await svc.EnqueueAsync("7", """{"brightness":80}""");
        await reconnected.Task.WaitAsync(DrainTimeout);

        // Assert — ConnectAsync was called at least once (reconnect attempted)
        callCount.Should().BeGreaterThanOrEqualTo(1, "service must reconnect when IsConnected is false");
    }

    // ── MP-03: EnqueueRawAsync → publishes to arbitrary topic with given retain flag ──
    [Fact]
    public async Task MqttPublisherService_EnqueueRaw_PublishesToGivenTopic()
    {
        var fakeClient = new Mock<IMqttClient>();
        fakeClient.Setup(c => c.IsConnected).Returns(true);

        var published = new TaskCompletionSource();
        fakeClient
            .Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(0, MqttClientPublishReasonCode.Success, string.Empty, null))
            .Callback(() => published.TrySetResult());

        var logger = new Mock<ILogger<MqttPublisherService>>().Object;
        var svc = new MqttPublisherService(fakeClient.Object, logger);

        await svc.EnqueueRawAsync("registry/13", """{"deviceId":13,"type":"AirConditioner"}""", retain: true);
        await published.Task.WaitAsync(DrainTimeout);

        fakeClient.Verify(c => c.PublishAsync(
            It.Is<MqttApplicationMessage>(m =>
                m.Topic == "registry/13" &&
                m.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtLeastOnce &&
                m.Retain == true),
            It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}
