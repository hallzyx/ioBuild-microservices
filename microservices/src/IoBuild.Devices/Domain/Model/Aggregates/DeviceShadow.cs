namespace IoBuild.Devices.Domain.Model.Aggregates;

/// <summary>
/// Desired-state shadow for a device (ADR-B3).
/// One row per device; upserted every time an owner issues a command.
///
/// Schema: device_shadows (device_id INT PK, desired_json LONGTEXT, reported_json LONGTEXT NULL,
///         updated_by_user_id INT, updated_at DATETIME).
///
/// Reported shadow is seeded as NULL — no reported-state ingest is wired in this change (out of scope).
/// MySQL-safe: plain PK, NO filtered/partial indexes (avoids Change A trap, ADR-B3).
/// </summary>
public class DeviceShadow
{
    /// <summary>Primary key — equals the device's Id (FK to Devices.device_id).</summary>
    public int DeviceId { get; set; }

    /// <summary>JSON dictionary of desired attribute values (e.g. {"targetTemperature":22}).</summary>
    public string DesiredJson { get; set; } = "{}";

    /// <summary>
    /// JSON dictionary of last reported attribute values from the device.
    /// NULL until a reported-state ingest mechanism is wired (future change).
    /// </summary>
    public string? ReportedJson { get; set; }

    /// <summary>UserId of the owner who last issued a command to this device.</summary>
    public int UpdatedByUserId { get; set; }

    /// <summary>UTC timestamp of the last command write.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
