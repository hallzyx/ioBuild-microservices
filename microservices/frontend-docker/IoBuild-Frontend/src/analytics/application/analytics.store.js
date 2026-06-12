import { defineStore } from "pinia";
import { ref } from "vue";
import { AnalyticsApi } from "../infrastructure/analytics-api.js";
import { DeviceApi } from "@/devices/infrastructure/device-api.js";

const analyticsApi = new AnalyticsApi();
const deviceApi = new DeviceApi();

export const useAnalyticsStore = defineStore("analytics", () => {
    const builderDashboard = ref(null);
    const ownerDashboard = ref(null);
    const historicalData = ref([]);
    const loading = ref(false);
    const errors = ref([]);

    const deviceEnergyReadings = ref([]);
    const deviceStatus = ref(null);
    const selectedDeviceId = ref(null);
    const devices = ref([]);
    const telemetryLoading = ref(false);

    async function fetchBuilderDashboard(builderId) {
        loading.value = true;
        try {
            builderDashboard.value = await analyticsApi.getBuilderDashboard(builderId);
        } catch (error) {
            errors.value.push(error);
            console.error('Error fetching builder dashboard:', error);
        } finally {
            loading.value = false;
        }
    }

    async function fetchOwnerDashboard(ownerId) {
        loading.value = true;
        try {
            ownerDashboard.value = await analyticsApi.getOwnerDashboard(ownerId);
        } catch (error) {
            errors.value.push(error);
            console.error('Error fetching owner dashboard:', error);
        } finally {
            loading.value = false;
        }
    }

    async function fetchHistoricalData(projectId, dataType, startDate, endDate) {
        loading.value = true;
        try {
            historicalData.value = await analyticsApi.getHistoricalData(projectId, dataType, startDate, endDate);
        } catch (error) {
            errors.value.push(error);
            console.error('Error fetching historical data:', error);
        } finally {
            loading.value = false;
        }
    }

    async function fetchDevices() {
        try {
            const result = await deviceApi.getAllDevices();
            devices.value = result;
            // Auto-select first device when devices are loaded
            if (result.length > 0 && selectedDeviceId.value === null) {
                selectDevice(result[0].id);
            }
        } catch (error) {
            errors.value.push(error);
            console.error('Error fetching devices for telemetry:', error);
        }
    }

    async function fetchDeviceEnergy(deviceId, from, to) {
        telemetryLoading.value = true;
        try {
            deviceEnergyReadings.value = await analyticsApi.getDeviceEnergy(deviceId, from, to);
        } catch (error) {
            errors.value.push(error);
            console.error('Error fetching device energy:', error);
            deviceEnergyReadings.value = [];
        } finally {
            telemetryLoading.value = false;
        }
    }

    async function fetchDeviceStatus(deviceId) {
        try {
            deviceStatus.value = await analyticsApi.getDeviceStatus(deviceId);
        } catch (error) {
            errors.value.push(error);
            console.error('Error fetching device status:', error);
            deviceStatus.value = null;
        }
    }

    function selectDevice(deviceId) {
        selectedDeviceId.value = deviceId;
    }

    function clearErrors() {
        errors.value = [];
    }

    return {
        builderDashboard,
        ownerDashboard,
        historicalData,
        loading,
        errors,
        deviceEnergyReadings,
        deviceStatus,
        selectedDeviceId,
        devices,
        telemetryLoading,
        fetchBuilderDashboard,
        fetchOwnerDashboard,
        fetchHistoricalData,
        fetchDevices,
        fetchDeviceEnergy,
        fetchDeviceStatus,
        selectDevice,
        clearErrors
    };
});

export default useAnalyticsStore;
