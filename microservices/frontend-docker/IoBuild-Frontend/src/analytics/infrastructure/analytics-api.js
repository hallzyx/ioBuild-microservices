import { BaseApi } from "../../shared/infrastructure/base-api.js";
import { BuilderDashboardAssembler } from "./builder-dashboard.assembler.js";
import { OwnerDashboardAssembler } from "./owner-dashboard.assembler.js";
import { HistoricalDataAssembler } from "./historical-data.assembler.js";

const analyticsBasePath = import.meta.env.VITE_ANALYTICS_ENDPOINT_PATH;

/**
 * Analytics API class to interact with analytics endpoints
 * @extends BaseApi
 * @class
 * @example
 * const api = new AnalyticsApi();
 * api.getBuilderDashboard(1).then(dashboard => console.log(dashboard));
 */
export class AnalyticsApi extends BaseApi {
    constructor() {
        super();
    }

    /**
     * Get dashboard metrics by user ID and role
     * @param {number} userId - The user ID (builder or owner)
     * @param {string} role - The user role ('builder' or 'owner')
     * @returns {Promise<BuilderDashboard|OwnerDashboard>}
     */
    async getDashboardMetrics(userId, role) {
        try {
            const response = await this.http.get(`${analyticsBasePath}/metrics/${userId}?role=${role}`);
            
            if (role === 'builder') {
                return BuilderDashboardAssembler.toEntity(response.data);
            } else {
                return OwnerDashboardAssembler.toEntity(response.data);
            }
        } catch (error) {
            console.error('Error fetching dashboard metrics:', error);
            throw error;
        }
    }

    /**
     * Get builder dashboard analytics (uses new endpoint)
     * @param {number} builderId - The builder ID
     * @returns {Promise<BuilderDashboard>}
     */
    async getBuilderDashboard(builderId) {
        return this.getDashboardMetrics(builderId, 'builder');
    }

    /**
     * Get owner dashboard analytics (uses new endpoint)
     * @param {number} ownerId - The owner ID
     * @returns {Promise<OwnerDashboard>}
     */
    async getOwnerDashboard(ownerId) {
        return this.getDashboardMetrics(ownerId, 'owner');
    }

    /**
     * Get analytical insights for specific metrics
     * @param {number} projectId - The project ID
     * @param {string} metric - Type of metric (e.g., 'temperature', 'energy', 'water')
     * @param {Date} startDate - Start date (optional)
     * @param {Date} endDate - End date (optional)
     * @returns {Promise<Array<HistoricalDataPoint>>}
     */
    async getAnalyticalInsights(projectId, metric, startDate = null, endDate = null) {
        try {
            const params = new URLSearchParams({
                projectId: projectId.toString(),
                metric
            });
            
            if (startDate) {
                params.append('startDate', startDate.toISOString());
            }
            if (endDate) {
                params.append('endDate', endDate.toISOString());
            }
            
            const response = await this.http.get(`${analyticsBasePath}/insights?${params}`);
            return HistoricalDataAssembler.toEntityList(response.data);
        } catch (error) {
            console.error('Error fetching analytical insights:', error);
            throw error;
        }
    }

    /**
     * Get historical data for a project (uses new insights endpoint)
     * @param {number} projectId - The project ID
     * @param {string} dataType - Type of data (e.g., 'temperature', 'energy')
     * @param {Date} startDate - Start date
     * @param {Date} endDate - End date
     * @returns {Promise<Array<HistoricalDataPoint>>}
     */
    async getHistoricalData(projectId, dataType, startDate, endDate) {
        return this.getAnalyticalInsights(projectId, dataType, startDate, endDate);
    }
}
