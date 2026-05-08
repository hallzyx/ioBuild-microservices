namespace IoBuild.Analytics.Domain.Model.Queries;

public record GetHistoricalDataQuery(int ProjectId, string Metric, DateTime StartDate, DateTime EndDate);
