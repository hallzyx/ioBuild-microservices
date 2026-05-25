using IoBuild.Devices.Infrastructure.InfluxDB;
using IoBuild.Devices.Infrastructure.Mqtt;
using FluentAssertions;

namespace IoBuild.Devices.Tests.UnitTests;

public class TelemetryInfrastructureTests
{
    [Fact]
    public void MqttOptions_ShouldHaveDefaultValues()
    {
        var options = new MqttOptions();

        options.Host.Should().Be("mosquitto");
        options.Port.Should().Be(1883);
        options.Topic.Should().Be("telemetry/#");
        options.ClientId.Should().Be("iobuild-telemetry-worker");
    }

    [Fact]
    public void MqttOptions_ShouldHaveSectionName()
    {
        MqttOptions.SectionName.Should().Be("Mqtt");
    }

    [Fact]
    public void InfluxDbOptions_ShouldHaveDefaultValues()
    {
        var options = new InfluxDbOptions();

        options.Host.Should().Be("influxdb");
        options.Port.Should().Be(8086);
        options.Token.Should().BeNull();
        options.Org.Should().Be("iobuild");
        options.Bucket.Should().Be("iobuild-telemetry");
    }

    [Fact]
    public void InfluxDbOptions_ShouldHaveSectionName()
    {
        InfluxDbOptions.SectionName.Should().Be("InfluxDb");
    }

    [Fact]
    public void TelemetryPoint_ShouldStoreAndExposeProperties()
    {
        var now = DateTime.UtcNow;
        var point = new TelemetryPoint
        {
            DeviceId = "sensor-01",
            Location = "Sector-A",
            EnergyKwh = 1.5,
            TemperatureC = 22.5,
            VoltageV = 220.0,
            Status = "online",
            Timestamp = now
        };

        point.DeviceId.Should().Be("sensor-01");
        point.Location.Should().Be("Sector-A");
        point.EnergyKwh.Should().Be(1.5);
        point.TemperatureC.Should().Be(22.5);
        point.VoltageV.Should().Be(220.0);
        point.Status.Should().Be("online");
        point.Timestamp.Should().Be(now);
    }

    [Fact]
    public void TelemetryPoint_ShouldBeDecoratedWithMeasurementAttribute()
    {
        var attrs = typeof(TelemetryPoint).GetCustomAttributes(inherit: false);
        attrs.Should().Contain(a => a.GetType().Name == "Measurement");
    }

    [Fact]
    public void ITelemetryWriteService_ShouldDefineWriteAsyncMethod()
    {
        var method = typeof(ITelemetryWriteService).GetMethod("WriteAsync");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(TelemetryPoint));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
        parameters[1].IsOptional.Should().BeTrue();
    }
}
