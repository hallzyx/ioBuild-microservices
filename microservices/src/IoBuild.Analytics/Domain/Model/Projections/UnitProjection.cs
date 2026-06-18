namespace IoBuild.Analytics.Domain.Model.Projections;

/// <summary>
/// Current-state projection of a Unit aggregate (ADR-6, REQ-RM-01).
/// Populated by AnalyticsEventConsumer from UnitCreated events.
/// </summary>
public class UnitProjection
{
    public int UnitId { get; set; }             // PK — natural key from IoBuild.Projects
    public int ProjectId { get; set; }          // Links to ProjectProjection
    public int BuilderUserId { get; set; }      // Drives builder dashboard queries
    public int? OwnerUserId { get; set; }       // Drives owner MyUnitsCount
    public string Status { get; set; } = string.Empty;
    public DateTime LastEventAt { get; set; }   // LWW guard (ADR-5)
}
