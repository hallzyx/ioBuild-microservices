import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { ProjectApi } from "../infrastructure/project-api.js";
import { ProjectAssembler } from "../infrastructure/project.assembler.js";

const projectApi = new ProjectApi();

// Resolve the authenticated builder's id from the session (IAM stores it in
// localStorage after login). Returns null when there's no valid session, so
// callers can fail closed instead of falling back to another builder's data.
function getCurrentBuilderId() {
    try {
        const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
        return currentUser?.id ?? null;
    } catch (error) {
        console.error('[projects] could not read current user:', error);
        return null;
    }
}

export const useProjectStore = defineStore("projects", () => {
    const projects = ref([]);
    const errors = ref([]);
    const projectsLoaded = ref(false);

    const projectsCount = computed(() => (projectsLoaded ? projects.value.length : 0));

    function fetchProjects() {
        const builderId = getCurrentBuilderId();
        if (!builderId) {
            projects.value = [];
            projectsLoaded.value = true;
            return Promise.resolve();
        }
        return projectApi
            .getProjectsByBuilderId(builderId)
            .then((response) => {
                projects.value = ProjectAssembler.toEntitiesFromResponse(response);
                projectsLoaded.value = true;
            })
            .catch((error) => {
                errors.value.push(error);
            });
    }

    function getProjectById(id) {
        const idInt = parseInt(id);
        return projects.value.find((p) => p.id === idInt);
    }

    function addProject(project) {
        // Stamp ownership with the authenticated builder so the project belongs
        // to its creator (not the entity's default builderId).
        const builderId = getCurrentBuilderId();
        projectApi
            .createProject({ ...project, builderId })
            .then((response) => {
                const resource = response.data;
                const newProject = ProjectAssembler.toEntityFromResource(resource);
                projects.value.push(newProject);
            })
            .catch((error) => errors.value.push(error));
    }

    function updateProject(project) {
        return projectApi
            .updateProject(project)
            .then((response) => {
                // Patch in place when the API echoes the entity; the backend may
                // return 204 (no body), so the refetch below is the real fix.
                if (response?.data) {
                    const updated = ProjectAssembler.toEntityFromResource(response.data);
                    const index = projects.value.findIndex((p) => p.id === updated.id);
                    if (index !== -1) projects.value[index] = updated;
                }
                projectsLoaded.value = false;
            })
            .catch((error) => errors.value.push(error));
    }

    function deleteProject(project) {
        return projectApi
            .deleteProject(project.id)
            .then(() => {
                const index = projects.value.findIndex((p) => p.id === project.id);
                if (index !== -1) projects.value.splice(index, 1);
                return fetchProjects();
            })
            .catch((error) => errors.value.push(error));
    }

    const structureLoading = ref(false);
    const structureError = ref(null);

    async function defineProjectStructure(projectId, payload) {
        structureLoading.value = true;
        structureError.value = null;
        try {
            const response = await projectApi.defineStructure(projectId, payload);
            return response;
        } catch (error) {
            structureError.value = error;
            throw error;
        } finally {
            structureLoading.value = false;
        }
    }

    async function assignUnitOwner(unitId, ownerEmail) {
        const response = await projectApi.patchUnitOwner(unitId, ownerEmail);
        return response.data;
    }

    return {
        projects,
        errors,
        projectsLoaded,
        projectsCount,
        structureLoading,
        structureError,
        fetchProjects,
        getProjectById,
        addProject,
        updateProject,
        deleteProject,
        defineProjectStructure,
        assignUnitOwner,
    };
});

export default useProjectStore;
