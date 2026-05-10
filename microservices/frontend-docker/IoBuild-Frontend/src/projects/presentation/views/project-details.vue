<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useConfirm } from "primevue/useconfirm";
import useProjectStore from "../../application/project.store.js";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const confirm = useConfirm();
const store = useProjectStore();
const project = ref(null);

onMounted(async () => {
  await store.fetchProjects();
  project.value = store.getProjectById(route.params.id);
});

const navigateBack = () => router.push({ name: "projects-management" });
const navigateToEdit = () =>
    router.push({ name: "projects-management-edit", params: { id: project.value.id } });

const confirmDelete = () => {
  confirm.require({
    message: t("projects.confirm-delete", { name: project.value.name }),
    header: t("projects.delete-header"),
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      try {
        await store.deleteProject(project.value);
        router.push({ name: "projects-management" });
      } catch (error) {
        console.error("Error deleting project:", error);
      }
    },
  });
};

function handleImageError(event) {
  event.target.src = "https://via.placeholder.com/32/10B981/ffffff?text=P"; // tamaño acorde al ícono
}
</script>

<template>
  <div v-if="project" class="p-6 max-w-3xl mx-auto bg-white rounded-lg shadow">
    <div class="flex justify-between items-center mb-6">
      <pv-button
          icon="pi pi-arrow-left"
          :label="t('projects.actions.go-back') || 'Go Back'"
          text
          @click="navigateBack"
      />
      <h1 class="text-2xl font-semibold text-center flex-1">{{ project.name }}</h1>
      <div class="flex items-center gap-2">
        <pv-button icon="pi pi-pencil" text rounded @click="navigateToEdit" />
        <pv-button icon="pi pi-trash" text rounded severity="danger" @click="confirmDelete" />
      </div>
    </div>
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 justify-content-center">
      <div class="lg:col-span-2 order-1">
        <div class="w-full aspect-video overflow-hidden rounded-lg shadow-lg">
          <img
              :src="project.imageUrl"
              :alt="project.name"
              class="w-full h-full object-cover transition duration-300 hover:scale-[1.03]"
              @error="handleImageError"
              loading="lazy"
          />
        </div>
      </div>

      <div class="lg:col-span-1 space-y-4 text-gray-700 order-2">
        <div class="flex items-center justify-between p-3 bg-green-50 rounded-lg border-l-4 border-green-500 shadow-sm justify-content-center">
          <span class="font-semibold">{{ t("projects.fields.status") }}:</span>
          <span class="px-3 py-1 text-sm font-bold rounded-full bg-green-600 text-white">{{ project.status }}</span>
        </div>

        <div class="space-y-3 p-3 bg-gray-50 rounded-lg shadow-sm">
          <p class="flex justify-between border-b pb-1">
            <span class="font-semibold">{{ t("projects.fields.total-units") }}:</span>
            <span class="font-medium text-gray-900">{{ project.totalUnits }}</span>
          </p>
          <p class="flex justify-between border-b pb-1">
            <span class="font-semibold">{{ t("projects.fields.occupied-units") }}:</span>
            <span class="font-medium text-gray-900">{{ project.occupiedUnits }}</span>
          </p>
          <p class="flex justify-between border-b pb-1">
            <span class="font-semibold">{{ t("projects.fields.created-date") }}:</span>
            <span class="font-medium text-gray-900">{{ project.createdDate }}</span>
          </p>
          <p class="flex justify-between">
            <span class="font-semibold">{{ t("projects.fields.description") }}:</span>
            <span class="font-medium text-gray-900">{{ project.description }}</span>
          </p>
          <p class="pt-2 text-sm text-gray-500">
            <strong>{{ t("projects.fields.location") }}:</strong> {{ project.location }}
          </p>
        </div>
      </div>
    </div>
  </div>
  <p v-else class="text-gray-500 text-center">{{ t("projects.messages.no-projects") }}</p>
</template>

<style scoped>

</style>