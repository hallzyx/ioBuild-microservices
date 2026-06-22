using System.Text.Json;

namespace IoBuild.Devices.Interfaces.REST.Resources;

/// <summary>
/// Request body for POST /api/v1/devices/{id}/command (ADR-B5).
/// Wire key is <c>attribute</c> — intentional (design wins over spec's <c>attributeName</c>).
/// </summary>
public record SendCommandResource(string Attribute, JsonElement Value);

/// <summary>
/// Response body for a successful POST /api/v1/devices/{id}/command (200 OK).
/// </summary>
public record CommandResultResource(
    int DeviceId,
    string Attribute,
    JsonElement Value,
    DateTime AcceptedAt);
