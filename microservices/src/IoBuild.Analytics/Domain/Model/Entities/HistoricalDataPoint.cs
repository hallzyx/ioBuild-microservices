namespace IoBuild.Analytics.Domain.Model.Entities;

public class HistoricalDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string Metric { get; set; } = string.Empty;
}
