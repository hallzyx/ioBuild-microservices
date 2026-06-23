import { OwnerDashboard } from "../domain/model/owner-dashboard.entity.js";
import { HistoricalDataPoint } from "../domain/model/historical-data-point.entity.js";

export class OwnerDashboardAssembler {
    static toEntity(resource) {
        return new OwnerDashboard({
            totalDevices: resource.totalDevices,
            onlineDevices: resource.onlineDevices,
            offlineDevices: resource.offlineDevices,
            alertsCount: resource.alertsCount,
            myUnitsCount: resource.myUnitsCount,
            energyThisMonth: resource.energyThisMonth,
            temperatureAvg: resource.temperatureAvg,
            waterUsageThisMonth: resource.waterUsageThisMonth,
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
            dailyEnergyConsumption: resource.dailyEnergyConsumption.map(point => 
                new HistoricalDataPoint({
                    timestamp: new Date(point.timestamp),
                    value: point.value,
                    type: point.type
                })
            ),
            waterUsageWeekly: resource.waterUsageWeekly.map(point => 
                new HistoricalDataPoint({
                    timestamp: new Date(point.timestamp),
                    value: point.value,
                    type: point.type
                })
            ),
            deviceHealthStatus: (resource.deviceHealthStatus || []).map(d => ({
                ...d
                // `type` is preserved from the backend payload (catalog code, e.g. "AirConditioner")
                // so the owner control panel can resolve controllable attributes. Do NOT override it
                // with a name-based heuristic — that produced "other" and broke the Control column.
                // healthPercentage intentionally omitted: no real signal on the backend (REQ-3, ADR-OD-3).
            })),
            myUnitsDetails: resource.myUnitsDetails
        });
    }
}
