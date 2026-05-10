import { defineStore } from "pinia";
import { ref } from "vue";
import { AnalyticsApi } from "../infrastructure/analytics-api.js";

const analyticsApi = new AnalyticsApi();

export const useAnalyticsStore = defineStore("analytics", () => {
    const builderDashboard = ref(null);
    const ownerDashboard = ref(null);
    const historicalData = ref([]);
    const loading = ref(false);
    const errors = ref([]);

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

    function clearErrors() {
        errors.value = [];
    }

    return {
        builderDashboard,
        ownerDashboard,
        historicalData,
        loading,
        errors,
        fetchBuilderDashboard,
        fetchOwnerDashboard,
        fetchHistoricalData,
        clearErrors
    };
});

export default useAnalyticsStore;
