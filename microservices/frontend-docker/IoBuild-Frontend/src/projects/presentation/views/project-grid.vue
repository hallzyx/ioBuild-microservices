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
  <div class="p-3 lg:p-4">
    <div class="flex items-center justify-between mb-3">
      <h1 class="text-xl font-semibold lg:text-2xl">{{ t("projects.title") }}</h1>
      <div class="ml-auto">
        <pv-button
          :label="t('projects.add')"
          icon="pi pi-plus"
          size="small"
          class="custom-green-button"
          @click="navigateToNew"
        />
      </div>
    </div>

    <div
      class="grid gap-[15px] grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 auto-rows-fr gap-4"
      v-if="store.projects.length"
    >
      <ProjectCard
        v-for="project in store.projects"
        :key="project.id"
        :project="project"
        @viewDetails="navigateToDetails(project.id)"
      />
    </div>
    <p v-else class="text-gray-500 text-sm lg:text-base">{{ t('projects.messages.no-projects') }}</p>
  </div>
</template>

<style scoped>
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