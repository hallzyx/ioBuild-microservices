<script setup>
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import useProjectStore from "../../application/project.store.js";
import ProjectCard from "../components/project-card.vue";

const { t } = useI18n();
const router = useRouter();
const store = useProjectStore();

onMounted(() => {
  if (!store.projectsLoaded) {
    store.fetchProjects();
  }
});

const navigateToNew = () => router.push({ name: "projects-management-new" });
const navigateToDetails = (id) => router.push({ name: "projects-management-details", params: { id } });
</script>

<template>
  <div class="p-3">
    <div class="flex align-items-center justify-content-between mb-3">
      <h1 class="text-xl font-semibold">{{ t("projects.title") }}</h1>
      <pv-button
        :label="t('projects.add')"
        icon="pi pi-plus"
        size="small"
        class="custom-green-button"
        @click="navigateToNew"
      />
    </div>

    <div
      class="project-grid"
      v-if="store.projects.length"
    >
      <ProjectCard
        v-for="project in store.projects"
        :key="project.id"
        :project="project"
        @viewDetails="navigateToDetails(project.id)"
      />
    </div>
    <p v-else class="text-gray-500 text-sm">{{ t('projects.messages.no-projects') }}</p>
  </div>
</template>

<style scoped>
/* Responsive card grid: 2 columns by default, up to 6 on very wide screens */
.project-grid {
  display: grid;
  gap: 15px;
  grid-template-columns: repeat(2, 1fr);
}

@media (min-width: 576px) {
  .project-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (min-width: 768px) {
  .project-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

@media (min-width: 992px) {
  .project-grid {
    grid-template-columns: repeat(5, 1fr);
  }
}

@media (min-width: 1200px) {
  .project-grid {
    grid-template-columns: repeat(6, 1fr);
  }
}

:deep(.custom-green-button) {
  background-color: #10B981 !important;
  border-color: #10B981 !important;
  color: white !important;
}

:deep(.custom-green-button:hover) {
  background-color: #059669 !important;
  border-color: #059669 !important;
}

:deep(.custom-green-button:focus) {
  box-shadow: 0 0 0 0.2rem rgba(16, 185, 129, 0.5) !important;
}
</style>
