namespace IoBuild.Analytics.Domain.Model.Projections;

/// <summary>
/// Current-state projection of a Device aggregate (ADR-6, REQ-RM-01).
/// Populated by AnalyticsEventConsumer from DeviceCreated/Updated/Deleted events.
///
/// Note on OwnerUserId: the Device aggregate has no owner concept — it carries
/// only ProjectId, not OwnerUserId. OwnerUserId is therefore always 0 here
/// (known source-data limitation; owner dashboard device counts = 0, not a regression).
/// Builder device counts ARE computable via the project_projection join.
/// </summary>
public class DeviceProjection
{
    public int DeviceId { get; set; }           // PK — natural key from IoBuild.Devices
    public int OwnerUserId { get; set; }        // Always 0 — see class doc
    public int? ProjectId { get; set; }         // Links to ProjectProjection for builder rollups
    public int? UnitId { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastEventAt { get; set; }   // LWW guard (ADR-5)
}
