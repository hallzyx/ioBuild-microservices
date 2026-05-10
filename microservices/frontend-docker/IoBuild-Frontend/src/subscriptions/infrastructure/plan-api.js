import { BaseApi } from "../../shared/infrastructure/base-api.js";
import { BaseEndpoint } from "../../shared/infrastructure/base-endpoint.js";

const plansEndpointPath = '/plans';

export class PlanApi extends BaseApi {
    #plansEndpoint;

    constructor() {
        super();
        this.#plansEndpoint = new BaseEndpoint(this, plansEndpointPath);
    }

    /**
     * Get all available plans
     */
    getAllPlans() {
        return this.#plansEndpoint.getAll();
    }

    /**
     * Get plan by ID
     */
    getPlanById(id) {
        return this.#plansEndpoint.getById(id);
    }
}

