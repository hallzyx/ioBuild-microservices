namespace IoBuild.Devices.Domain.Model.Commands;

/// <summary>
/// Command record for the owner-issued device actuation flow (ADR-B5).
/// Wire body key is <c>attribute</c> (ADR-B5 wins over spec's <c>attributeName</c>).
///
/// Routing: DevicesController → IDeviceActuationService.Handle().
/// </summary>
/// <param name="DeviceId">Target device id.</param>
/// <param name="Attribute">Catalog attribute name verbatim (e.g. "targetTemperature").</param>
/// <param name="Value">Scalar value — numeric, boolean, or enum string.</param>
/// <param name="RequestingUserId">UserId read from HttpContext.Items["UserId"].</param>
/// <param name="RequestingUserRole">Role read from HttpContext.Items["UserRole"].</param>
public record SendDeviceCommandCommand(
    int DeviceId,
    string Attribute,
    object Value,
    int RequestingUserId,
    string? RequestingUserRole);
