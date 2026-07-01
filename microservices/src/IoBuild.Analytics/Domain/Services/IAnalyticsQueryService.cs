using IoBuild.Analytics.Domain.Model.Aggregates;
using IoBuild.Analytics.Domain.Model.Entities;
using IoBuild.Analytics.Domain.Model.Queries;
using IoBuild.Analytics.Infrastructure.InfluxDB;

namespace IoBuild.Analytics.Domain.Services;

public interface IAnalyticsQueryService
{
    Task<BuilderMetrics?> Handle(GetBuilderDashboardQuery query);
    Task<OwnerMetrics?> Handle(GetOwnerDashboardQuery query);
    Task<IEnumerable<HistoricalDataPoint>> Handle(GetHistoricalDataQuery query);
    Task<IEnumerable<EnergyMinutePoint>> Handle(GetBuilderLiveEnergyQuery query);
    Task<IEnumerable<EnergyMinutePoint>> Handle(GetOwnerLiveEnergyQuery query);
}
