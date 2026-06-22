using IoBuild.Devices.Domain.Model.Commands;

namespace IoBuild.Devices.Domain.Services;

/// <summary>
/// Result returned by <see cref="IDeviceActuationService.Handle"/>.
/// </summary>
/// <param name="StatusCode">HTTP status code: 200, 400, 403, or 404.</param>
/// <param name="AcceptedAt">UTC timestamp when the command was accepted (set on 200 only).</param>
/// <param name="ErrorMessage">Human-readable error description (set on 400, 403, 404).</param>
public record ActuationResult(int StatusCode, DateTime? AcceptedAt = null, string? ErrorMessage = null)
{
    public static ActuationResult Ok(DateTime acceptedAt) => new(200, acceptedAt);
    public static ActuationResult BadRequest(string message) => new(400, ErrorMessage: message);
    public static ActuationResult Forbidden(string message) => new(403, ErrorMessage: message);
    public static ActuationResult NotFound(string message) => new(404, ErrorMessage: message);
}

/// <summary>
/// Port for the owner-side device actuation use case (ADR-B5).
/// Separate from IDeviceCommandService which handles registry CRUD (create/update/delete devices).
/// </summary>
public interface IDeviceActuationService
{
    /// <summary>
    /// Handles a <see cref="SendDeviceCommandCommand"/>.
    /// Validates role + owner projection + catalog range, upserts shadow, emits outbox event,
    /// enqueues MQTT publish.
    /// </summary>
    Task<ActuationResult> Handle(SendDeviceCommandCommand command);
}
