namespace IoBuild.Devices.Infrastructure.InfluxDB;

public interface ITelemetryWriteService
{
    Task WriteAsync(TelemetryPoint point, CancellationToken ct = default);
}
