import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { ProjectApi } from "../infrastructure/project-api.js";
import { ProjectAssembler } from "../infrastructure/project.assembler.js";

const projectApi = new ProjectApi();

export const useProjectStore = defineStore("projects", () => {
    const projects = ref([]);
    const errors = ref([]);
    const projectsLoaded = ref(false);
    const builderId = 1;

    const projectsCount = computed(() => (projectsLoaded ? projects.value.length : 0));

    function fetchProjects() {
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
        projectApi
            .createProject(project)
            .then((response) => {
                const resource = response.data;
                const newProject = ProjectAssembler.toEntityFromResource(resource);
                projects.value.push(newProject);
            })
            .catch((error) => errors.value.push(error));
    }

    function updateProject(project) {
        projectApi
            .updateProject(project)
            .then((response) => {
                const resource = response.data;
                const updated = ProjectAssembler.toEntityFromResource(resource);
                const index = projects.value.findIndex((p) => p.id === updated.id);
                if (index !== -1) projects.value[index] = updated;
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

    return {
        projects,
        errors,
        projectsLoaded,
        projectsCount,
        fetchProjects,
        getProjectById,
        addProject,
        updateProject,
        deleteProject,
    };
});

export default useProjectStore;
