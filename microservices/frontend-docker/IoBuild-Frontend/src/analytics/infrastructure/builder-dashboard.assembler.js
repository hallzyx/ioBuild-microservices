import { BuilderDashboard } from "../domain/model/builder-dashboard.entity.js";
import { HistoricalDataPoint } from "../domain/model/historical-data-point.entity.js";

export class BuilderDashboardAssembler {
    static toEntity(resource) {
        return new BuilderDashboard({
            totalDevices: resource.totalDevices,
            onlineDevices: resource.onlineDevices,
            offlineDevices: resource.offlineDevices,
            alertsCount: resource.alertsCount,
            activeProjectsCount: resource.activeProjectsCount,
            totalUnits: resource.totalUnits,
            occupiedUnits: resource.occupiedUnits,
            occupancyRate: resource.occupancyRate,
            energyEfficiencyAvg: resource.energyEfficiencyAvg,
            temperatureHistory: resource.temperatureHistory.map(point => 
                new HistoricalDataPoint({
                    timestamp: new Date(point.timestamp),
                    value: point.value,
                    type: point.type
                })
            ),
            energyHistory: resource.energyHistory.map(point => 
                new HistoricalDataPoint({
                    timestamp: new Date(point.timestamp),
                    value: point.value,
                    type: point.type
                })
            ),
            hourlyEnergyData: resource.hourlyEnergyData.map(point => 
                new HistoricalDataPoint({
                    timestamp: new Date(point.timestamp),
                    value: point.value,
                    type: point.type
                })
            ),
            monthlyOccupancy: resource.monthlyOccupancy,
            devicesByType: resource.devicesByType,
            projectsOverview: resource.projectsOverview
        });
    }
}
