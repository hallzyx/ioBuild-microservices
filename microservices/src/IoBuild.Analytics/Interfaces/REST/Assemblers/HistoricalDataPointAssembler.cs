using IoBuild.Analytics.Domain.Model.Entities;
using IoBuild.Analytics.Interfaces.REST.Resources;

namespace IoBuild.Analytics.Interfaces.REST.Assemblers;

public static class HistoricalDataPointAssembler
{
    public static HistoricalDataPointResource ToResource(HistoricalDataPoint point)
    {
        return new HistoricalDataPointResource(point.Timestamp, point.Value, point.Metric);
    }
}
