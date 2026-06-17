namespace IoBuild.Analytics.Domain.Model.Projections;

/// <summary>
/// Current-state projection of a Project aggregate (ADR-6, REQ-RM-01).
/// Populated by AnalyticsEventConsumer from ProjectCreated/Updated events.
/// </summary>
public class ProjectProjection
{
    public int ProjectId { get; set; }          // PK — natural key from IoBuild.Projects
    public int BuilderUserId { get; set; }      // Drives builder dashboard queries
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastEventAt { get; set; }   // LWW guard (ADR-5)
}
