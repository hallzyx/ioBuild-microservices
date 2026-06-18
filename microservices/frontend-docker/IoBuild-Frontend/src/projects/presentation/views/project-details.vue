<script setup>
import { onMounted, ref, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useConfirm } from "primevue/useconfirm";
import useProjectStore from "../../application/project.store.js";
import DefineStructureDialog from "../components/define-structure-dialog.vue";
import { ProjectApi } from "../../infrastructure/project-api.js";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const confirm = useConfirm();
const store = useProjectStore();
const projectApi = new ProjectApi();
const project = ref(null);
const units = ref([]);
const showStructureDialog = ref(false);

async function loadUnits() {
  try {
    units.value = await projectApi.getUnitsByProject(route.params.id);
  } catch (error) {
    console.error("Error loading units:", error);
    units.value = [];
  }
}

onMounted(async () => {
  await store.fetchProjects();
  project.value = store.getProjectById(route.params.id);
  await loadUnits();
});

// A project has a structure once it actually has units. Project.TotalUnits is a
// separate stored field that the define-structure flow does not update, so derive
// this from the fetched units instead.
const hasStructure = computed(() => units.value.length > 0);

// Group the project's units by floor for the structure display.
const structureGrid = computed(() => {
  const byFloor = new Map();
  for (const u of units.value) {
    if (!byFloor.has(u.floor)) byFloor.set(u.floor, []);
    byFloor.get(u.floor).push(u);
  }
  return [...byFloor.entries()]
    .sort((a, b) => a[0] - b[0])
    .map(([floor, list]) => ({
      floor,
      units: list.slice().sort((a, b) => (a.roomNumber || "").localeCompare(b.roomNumber || "")),
    }));
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

async function onStructureDefined() {
  await store.fetchProjects();
  project.value = store.getProjectById(route.params.id);
  await loadUnits();
}

function handleImageError(event) {
  event.target.src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='32' height='32'%3E%3Crect width='32' height='32' rx='4' fill='%2310B981'/%3E%3Ctext x='16' y='21' text-anchor='middle' fill='white' font-size='14' font-family='sans-serif'%3EP%3C/text%3E%3C/svg%3E";
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

        <!-- Define Structure — only visible when no structure exists yet -->
        <div v-if="!hasStructure" class="pt-2">
          <pv-button
              label="Define Structure"
              icon="pi pi-sitemap"
              class="w-full"
              severity="success"
              @click="showStructureDialog = true"
          />
        </div>
      </div>
    </div>

    <!-- Structure display — shown once the project has units -->
    <div v-if="hasStructure" class="mt-8">
      <div class="flex items-center gap-2 mb-4">
        <i class="pi pi-building text-emerald-600 text-lg"></i>
        <h2 class="text-lg font-semibold text-gray-800">Project Structure</h2>
        <pv-tag :value="`${units.length} units`" severity="success" />
      </div>

      <!-- Per-floor unit grid from the units API -->
      <div v-if="structureGrid.length > 0" class="space-y-4">
        <div v-for="row in structureGrid" :key="row.floor" class="p-3 bg-gray-50 rounded-lg">
          <p class="font-semibold text-gray-700 text-sm mb-2">Floor {{ row.floor }}</p>
          <div class="flex flex-wrap gap-2">
            <span
                v-for="unit in row.units"
                :key="unit.id"
                class="px-3 py-1 text-xs font-mono rounded border"
                :class="unit.ownerEmail
                  ? 'bg-blue-100 text-blue-800 border-blue-200'
                  : 'bg-emerald-100 text-emerald-800 border-emerald-200'"
                :title="unit.ownerEmail || 'unassigned'"
            >
              {{ unit.roomNumber }}<span v-if="unit.ownerEmail"> · {{ unit.ownerEmail }}</span>
            </span>
          </div>
        </div>
      </div>

      <!-- Fallback while units load -->
      <div v-else class="p-3 bg-emerald-50 rounded-lg border border-emerald-200 text-sm text-emerald-800">
        <i class="pi pi-info-circle mr-2"></i>
        This project has <strong>{{ project.totalUnits }}</strong> total unit(s)
        and <strong>{{ project.occupiedUnits }}</strong> occupied.
      </div>
    </div>
  </div>

  <p v-else class="text-gray-500 text-center">{{ t("projects.messages.no-projects") }}</p>

  <!-- Define Structure Dialog -->
  <DefineStructureDialog
      v-model:visible="showStructureDialog"
      :project-id="route.params.id"
      @structure-defined="onStructureDefined"
  />
</template>

<style scoped>

</style>
