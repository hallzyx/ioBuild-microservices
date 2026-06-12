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
                ...d,
                healthPercentage: d.status === 'Online' ? 92 + Math.floor(Math.random() * 8) : 30 + Math.floor(Math.random() * 40),
                type: d.deviceName?.includes('Temperature') ? 'temperature' :
                      d.deviceName?.includes('Energy') ? 'energy' :
                      d.deviceName?.includes('Water') ? 'water' : 'other'
            })),
            myUnitsDetails: resource.myUnitsDetails
        });
    }
}
