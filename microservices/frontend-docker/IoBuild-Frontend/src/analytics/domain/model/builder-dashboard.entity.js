export class BuilderDashboard {
    constructor({
        totalDevices = 0,
        onlineDevices = 0,
        offlineDevices = 0,
        alertsCount = 0,
        activeProjectsCount = 0,
        totalUnits = 0,
        occupiedUnits = 0,
        occupancyRate = 0,
        energyEfficiencyAvg = 0,
        temperatureHistory = [],
        energyHistory = [],
        hourlyEnergyData = [],
        monthlyOccupancy = [],
        devicesByType = {},
        projectsOverview = []
    } = {}) {
        this.totalDevices = totalDevices;
        this.onlineDevices = onlineDevices;
        this.offlineDevices = offlineDevices;
        this.alertsCount = alertsCount;
        this.activeProjectsCount = activeProjectsCount;
        this.totalUnits = totalUnits;
        this.occupiedUnits = occupiedUnits;
        this.occupancyRate = occupancyRate;
        this.energyEfficiencyAvg = energyEfficiencyAvg;
        this.temperatureHistory = temperatureHistory;
        this.energyHistory = energyHistory;
        this.hourlyEnergyData = hourlyEnergyData;
        this.monthlyOccupancy = monthlyOccupancy;
        this.devicesByType = devicesByType;
        this.projectsOverview = projectsOverview;
    }
}
