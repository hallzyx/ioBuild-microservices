namespace IoBuild.Analytics.Interfaces.REST.Resources;

public record HistoricalDataPointResource(DateTime Timestamp, double Value, string Metric);
