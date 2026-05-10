export class OwnerDashboard {
    constructor({
        totalDevices = 0,
        onlineDevices = 0,
        offlineDevices = 0,
        alertsCount = 0,
        myUnitsCount = 0,
        energyThisMonth = 0,
        temperatureAvg = 0,
        waterUsageThisMonth = 0,
        temperatureHistory = [],
        energyHistory = [],
        dailyEnergyConsumption = [],
        waterUsageWeekly = [],
        deviceHealthStatus = [],
        myUnitsDetails = []
    } = {}) {
        this.totalDevices = totalDevices;
        this.onlineDevices = onlineDevices;
        this.offlineDevices = offlineDevices;
        this.alertsCount = alertsCount;
        this.myUnitsCount = myUnitsCount;
        this.energyThisMonth = energyThisMonth;
        this.temperatureAvg = temperatureAvg;
        this.waterUsageThisMonth = waterUsageThisMonth;
        this.temperatureHistory = temperatureHistory;
        this.energyHistory = energyHistory;
        this.dailyEnergyConsumption = dailyEnergyConsumption;
        this.waterUsageWeekly = waterUsageWeekly;
        this.deviceHealthStatus = deviceHealthStatus;
        this.myUnitsDetails = myUnitsDetails;
    }
}
