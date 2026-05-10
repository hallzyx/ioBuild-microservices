import { BaseApi } from "../../shared/infrastructure/base-api.js";
import { BaseEndpoint } from "../../shared/infrastructure/base-endpoint.js";

const projectsEndpointPath = import.meta.env.VITE_PROJECTS_ENDPOINT_PATH;

export class ProjectApi extends BaseApi {
    #projectsEndpoint;

    constructor() {
        super();
        this.#projectsEndpoint = new BaseEndpoint(this, projectsEndpointPath);
    }

    async getProjectsByBuilderId(builderId) {
        const response = await this.#projectsEndpoint.getAll();
        const allProjects = response.data;
        const filtered = allProjects.filter(p => p.builderId === builderId);
        return { data: filtered };
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
}
