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
const unitsError = ref(null);
const showStructureDialog = ref(false);
const editMode = ref(false);
const pendingOwnerEmails = ref({});
const savingUnit = ref(null);

async function loadUnits() {
  unitsError.value = null;
  try {
    units.value = await projectApi.getUnitsByProject(route.params.id);
  } catch (error) {
    console.error("Error loading units:", error);
    unitsError.value = error;
  }
}

onMounted(async () => {
  await store.fetchProjects();
  project.value = store.getProjectById(route.params.id);
  await loadUnits();
});

// A project has a structure once it actually has units. Deriving this from the
// fetched units (rather than project.totalUnits) keeps the check robust even if
// the stored totals lag behind for legacy projects.
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

async function saveUnitOwner(unit) {
  const email = pendingOwnerEmails.value[unit.id];
  if (!email || !email.trim()) return;
  savingUnit.value = unit.id;
  try {
    const updated = await store.assignUnitOwner(unit.id, email.trim());
    const idx = units.value.findIndex(u => u.id === unit.id);
    if (idx !== -1) units.value[idx] = { ...units.value[idx], ...updated };
    delete pendingOwnerEmails.value[unit.id];
    await store.fetchProjects();
    project.value = store.getProjectById(route.params.id);
  } catch (error) {
    console.error("Error assigning unit owner:", error);
  } finally {
    savingUnit.value = null;
  }
}

// Clear a unit's owner email (PATCH with null). Frees the room so the project's
// occupied-units count decreases, mirroring the increment on assignment.
async function clearUnitOwner(unit) {
  savingUnit.value = unit.id;
  try {
    const updated = await store.assignUnitOwner(unit.id, null);
    const idx = units.value.findIndex(u => u.id === unit.id);
    // Force ownerEmail to null in case the API omits null fields from the resource.
    if (idx !== -1) units.value[idx] = { ...units.value[idx], ...updated, ownerEmail: null };
    await store.fetchProjects();
    project.value = store.getProjectById(route.params.id);
  } catch (error) {
    console.error("Error clearing unit owner:", error);
  } finally {
    savingUnit.value = null;
  }
}

function handleImageError(event) {
  event.target.src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='32' height='32'%3E%3Crect width='32' height='32' rx='4' fill='%2310B981'/%3E%3Ctext x='16' y='21' text-anchor='middle' fill='white' font-size='14' font-family='sans-serif'%3EP%3C/text%3E%3C/svg%3E";
}
</script>

<template>
  <!-- Single root element: <transition mode="out-in"> in the layout requires one
       root node, otherwise leaving this view leaves the next route blank. -->
  <div>
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
          <span class="px-3 py-1 text-sm font-bold rounded-full bg-green-600 text-white">{{ project.statusLabel }}</span>
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

        <!-- Define Structure — only visible when no structure exists yet AND fetch succeeded -->
        <div v-if="!hasStructure && !unitsError" class="pt-2">
          <pv-button
              label="Define Structure"
              icon="pi pi-sitemap"
              class="w-full"
              severity="success"
              @click="showStructureDialog = true"
          />
        </div>

        <!-- Units fetch error — prevents misreading a failed load as empty project -->
        <div v-if="unitsError" class="pt-2 p-3 bg-red-50 rounded-lg border border-red-200 text-sm text-red-700">
          <i class="pi pi-exclamation-circle mr-2"></i>
          Could not load project structure. Please refresh and try again.
        </div>
      </div>
    </div>

    <!-- Structure display — shown once the project has units -->
    <div v-if="hasStructure" class="mt-8">
      <div class="flex items-center gap-2 mb-4">
        <i class="pi pi-building text-emerald-600 text-lg"></i>
        <h2 class="text-lg font-semibold text-gray-800">Project Structure</h2>
        <pv-tag :value="`${units.length} units`" severity="success" />
        <pv-button
            :label="editMode ? 'Done' : 'Edit owners'"
            :icon="editMode ? 'pi pi-check' : 'pi pi-pencil'"
            :severity="editMode ? 'success' : 'secondary'"
            text
            size="small"
            class="ml-auto"
            @click="editMode = !editMode"
        />
      </div>

      <!-- Per-floor unit grid from the units API -->
      <div v-if="structureGrid.length > 0" class="space-y-4">
        <div v-for="row in structureGrid" :key="row.floor" class="p-3 bg-gray-50 rounded-lg">
          <p class="font-semibold text-gray-700 text-sm mb-2">Floor {{ row.floor }}</p>
          <div class="flex flex-wrap gap-2">
            <template v-for="unit in row.units" :key="unit.id">
              <!-- Edit mode: show inline input only for unassigned units -->
              <div
                  v-if="editMode && !unit.ownerEmail"
                  class="flex items-center gap-1 px-2 py-1 bg-yellow-50 border border-yellow-300 rounded text-xs"
              >
                <span class="font-mono text-gray-600 shrink-0">{{ unit.roomNumber }}</span>
                <pv-input-text
                    v-model="pendingOwnerEmails[unit.id]"
                    placeholder="owner@example.com"
                    class="text-xs"
                    style="width: 180px; padding: 2px 6px; font-size: 0.7rem;"
                    type="email"
                />
                <pv-button
                    icon="pi pi-check"
                    severity="success"
                    text
                    rounded
                    size="small"
                    :loading="savingUnit === unit.id"
                    :disabled="!pendingOwnerEmails[unit.id]"
                    @click="saveUnitOwner(unit)"
                />
              </div>
              <!-- Edit mode: assigned unit shows its email plus a clear button -->
              <div
                  v-else-if="editMode && unit.ownerEmail"
                  class="flex items-center gap-1 px-2 py-1 bg-blue-100 border border-blue-200 rounded text-xs"
              >
                <span class="font-mono text-blue-800 shrink-0">{{ unit.roomNumber }}</span>
                <span class="text-blue-700 truncate" style="max-width: 140px;">{{ unit.ownerEmail }}</span>
                <pv-button
                    icon="pi pi-times"
                    severity="danger"
                    text
                    rounded
                    size="small"
                    :loading="savingUnit === unit.id"
                    :title="t('common.clear') || 'Clear owner'"
                    @click="clearUnitOwner(unit)"
                />
              </div>
              <!-- Default badge (view mode) -->
              <span
                  v-else
                  class="px-3 py-1 text-xs font-mono rounded border"
                  :class="unit.ownerEmail
                    ? 'bg-blue-100 text-blue-800 border-blue-200'
                    : 'bg-emerald-100 text-emerald-800 border-emerald-200'"
                  :title="unit.ownerEmail || 'unassigned'"
              >
                {{ unit.roomNumber }}<span v-if="unit.ownerEmail"> · {{ unit.ownerEmail }}</span>
              </span>
            </template>
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
  </div>
</template>

<style scoped>

</style>
