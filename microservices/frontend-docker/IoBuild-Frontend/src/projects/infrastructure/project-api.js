import { BaseApi } from "../../shared/infrastructure/base-api.js";
import { BaseEndpoint } from "../../shared/infrastructure/base-endpoint.js";

const projectsEndpointPath = import.meta.env.VITE_PROJECTS_ENDPOINT_PATH;
const unitsEndpointPath = import.meta.env.VITE_UNITS_ENDPOINT_PATH || '/units';

export class ProjectApi extends BaseApi {
    #projectsEndpoint;

    constructor() {
        super();
        this.#projectsEndpoint = new BaseEndpoint(this, projectsEndpointPath);
    }

    async getProjectsByBuilderId(builderId) {
        const response = await this.#projectsEndpoint.getAll();
        return response;
    }

    getProjectById(id) {
        return this.#projectsEndpoint.getById(id);
    }

    createProject(resource) {
        return this.#projectsEndpoint.create(resource);
    }

    updateProject(resource) {
        return this.#projectsEndpoint.update(resource.id, resource);
    }

    deleteProject(id) {
        return this.#projectsEndpoint.delete(id);
    }

    defineStructure(projectId, payload) {
        return this.http.post(`${projectsEndpointPath}/${projectId}/structure`, payload);
    }

    async getUnitsByProject(projectId) {
        const response = await this.http.get(unitsEndpointPath, { params: { projectId } });
        return response.data;
    }

    patchUnitOwner(unitId, ownerEmail) {
        return this.http.patch(`${unitsEndpointPath}/${unitId}`, { ownerEmail });
    }
}
