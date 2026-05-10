/**
 * Projects Facade for Clients Module
 * This facade provides access to projects functionality following ACL pattern
 */
import { useProjectStore } from "../../projects/application/project.store.js";

export class ProjectsFacade {
    #projectStore;

    constructor() {
        this.#projectStore = useProjectStore();
    }

    /**
     * Get all projects
     * @returns {Array} Array of project entities
     */
    getProjects() {
        return this.#projectStore.projects;
    }

    /**
     * Fetch projects from API
     * @returns {Promise}
     */
    async fetchProjects() {
        if (!this.#projectStore.projectsLoaded) {
            await this.#projectStore.fetchProjects();
        }
        return this.#projectStore.projects;
    }

    /**
     * Get project by ID
     * @param {number} projectId
     * @returns {Object|undefined} Project entity
     */
    getProjectById(projectId) {
        return this.#projectStore.getProjectById(projectId);
    }

    /**
     * Get project name by ID
     * @param {number} projectId
     * @returns {string} Project name or empty string
     */
    getProjectNameById(projectId) {
        const project = this.getProjectById(projectId);
        return project ? project.name : '';
    }

    /**
     * Check if projects are loaded
     * @returns {boolean}
     */
    areProjectsLoaded() {
        return this.#projectStore.projectsLoaded;
    }
}

