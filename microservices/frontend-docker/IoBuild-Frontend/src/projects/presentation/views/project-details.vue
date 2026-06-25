<script setup>
import { onMounted, ref, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useConfirm } from "primevue/useconfirm";
import useProjectStore from "../../application/project.store.js";
import { useAnalyticsStore } from "../../../analytics/application/analytics.store.js";
import DefineStructureDialog from "../components/define-structure-dialog.vue";
import { ProjectApi } from "../../infrastructure/project-api.js";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const confirm = useConfirm();
const store = useProjectStore();
const analyticsStore = useAnalyticsStore();
const projectApi = new ProjectApi();
const project = ref(null);
const units = ref([]);
const unitsError = ref(null);
const showStructureDialog = ref(false);
const editMode = ref(false);
const pendingOwnerEmails = ref({});
const savingUnit = ref(null);
// Falls back to a branded placeholder when the project image fails to load.
const imageBroken = ref(false);

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

// How many units on a floor already have an owner assigned (for the floor header).
function occupiedCount(unitList) {
  return unitList.filter((u) => u.ownerEmail).length;
}

// Human-readable created date (raw value is an ISO timestamp).
function formatDate(iso) {
  if (!iso) return "—";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

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
  // Invalidate the analytics builder dashboard so navigating to it re-fetches
  // fresh data instead of showing a stale 0 from before structure was defined.
  analyticsStore.invalidateBuilderDashboard();
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

</script>

<template>
  <!-- Single root element: <transition mode="out-in"> in the layout requires one
       root node, otherwise leaving this view leaves the next route blank. -->
  <div>
  <div v-if="project" class="project-details-root p-6 bg-white">
    <div class="flex justify-content-between align-items-center mb-6">
      <pv-button
          icon="pi pi-arrow-left"
          :label="t('projects.actions.go-back') || 'Go Back'"
          text
          @click="navigateBack"
      />
      <h1 class="text-2xl font-semibold text-center flex-1">{{ project.name }}</h1>
      <div class="flex align-items-center gap-2">
        <pv-button icon="pi pi-pencil" text rounded @click="navigateToEdit" />
        <pv-button icon="pi pi-trash" text rounded severity="danger" @click="confirmDelete" />
      </div>
    </div>

    <div class="project-hero">
      <!-- Media -->
      <div class="project-hero__media">
        <img
            v-if="project.imageUrl && !imageBroken"
            :src="project.imageUrl"
            :alt="project.name"
            class="project-hero__img"
            @error="imageBroken = true"
            loading="lazy"
        />
        <div v-else class="project-hero__placeholder">
          <i class="pi pi-building"></i>
        </div>
      </div>

      <!-- Info -->
      <div class="project-hero__info">
        <div class="project-hero__statusrow">
          <span class="project-hero__status-label">{{ t("projects.fields.status") }}</span>
          <span class="project-hero__status-badge">{{ project.statusLabel }}</span>
        </div>

        <div class="project-hero__stats">
          <div class="stat">
            <span class="stat__value">{{ project.totalUnits }}</span>
            <span class="stat__label">{{ t("projects.fields.total-units") }}</span>
          </div>
          <div class="stat">
            <span class="stat__value">{{ project.occupiedUnits }}</span>
            <span class="stat__label">{{ t("projects.fields.occupied-units") }}</span>
          </div>
        </div>

        <dl class="project-hero__meta">
          <div class="meta-row">
            <dt><i class="pi pi-calendar"></i> {{ t("projects.fields.created-date") }}</dt>
            <dd>{{ formatDate(project.createdDate) }}</dd>
          </div>
          <div class="meta-row">
            <dt><i class="pi pi-map-marker"></i> {{ t("projects.fields.location") }}</dt>
            <dd>{{ project.location || "—" }}</dd>
          </div>
          <div v-if="project.description" class="meta-row meta-row--block">
            <dt><i class="pi pi-align-left"></i> {{ t("projects.fields.description") }}</dt>
            <dd>{{ project.description }}</dd>
          </div>
        </dl>

        <!-- Define Structure — only visible when no structure exists yet AND fetch succeeded -->
        <div v-if="!hasStructure && !unitsError">
          <pv-button
              label="Define Structure"
              icon="pi pi-sitemap"
              class="w-full"
              severity="success"
              @click="showStructureDialog = true"
          />
        </div>

        <!-- Units fetch error — prevents misreading a failed load as empty project -->
        <div v-if="unitsError" class="project-hero__error">
          <i class="pi pi-exclamation-circle"></i>
          Could not load project structure. Please refresh and try again.
        </div>
      </div>
    </div>

    <!-- Structure display — shown once the project has units -->
    <div v-if="hasStructure" class="mt-8">
      <div class="flex align-items-center gap-2 mb-4">
        <i class="pi pi-building section-icon text-lg"></i>
        <h2 class="text-lg font-semibold text-gray-800">Project Structure</h2>
        <pv-tag :value="`${units.length} units`" severity="success" />
        <pv-button
            :label="editMode ? 'Done' : 'Edit owners'"
            :icon="editMode ? 'pi pi-check' : 'pi pi-pencil'"
            :severity="editMode ? 'success' : 'secondary'"
            text
            size="small"
            class="structure-edit-btn"
            @click="editMode = !editMode"
        />
      </div>

      <!-- Per-floor unit grid from the units API -->
      <div v-if="structureGrid.length > 0" class="structure-floors">
        <section v-for="row in structureGrid" :key="row.floor" class="floor-card">
          <header class="floor-card__head">
            <span class="floor-card__name">Floor {{ row.floor }}</span>
            <span class="floor-card__occ">
              <i class="pi pi-user"></i>
              {{ occupiedCount(row.units) }}/{{ row.units.length }} occupied
            </span>
          </header>

          <div class="unit-grid">
            <template v-for="unit in row.units" :key="unit.id">
              <!-- Edit mode: assign an owner to a vacant unit -->
              <div v-if="editMode && !unit.ownerEmail" class="unit-card unit-card--edit">
                <span class="unit-card__no">{{ unit.roomNumber }}</span>
                <div class="unit-card__editrow">
                  <pv-input-text
                      v-model="pendingOwnerEmails[unit.id]"
                      placeholder="owner@example.com"
                      type="email"
                      class="unit-card__input"
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
              </div>

              <!-- Edit mode: assigned unit shows its owner plus a clear action -->
              <div v-else-if="editMode && unit.ownerEmail" class="unit-card unit-card--occupied unit-card--editrow-inline">
                <span class="unit-card__no">{{ unit.roomNumber }}</span>
                <span class="unit-card__email" :title="unit.ownerEmail">{{ unit.ownerEmail }}</span>
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

              <!-- View mode -->
              <div
                  v-else
                  class="unit-card"
                  :class="unit.ownerEmail ? 'unit-card--occupied' : 'unit-card--vacant'"
                  :title="unit.ownerEmail || 'Unassigned'"
              >
                <div class="unit-card__top">
                  <i class="pi" :class="unit.ownerEmail ? 'pi-user' : 'pi-home'"></i>
                  <span class="unit-card__no">{{ unit.roomNumber }}</span>
                </div>
                <span class="unit-card__status">{{ unit.ownerEmail || 'Vacant' }}</span>
              </div>
            </template>
          </div>
        </section>
      </div>

      <!-- Fallback while units load -->
      <div v-else class="units-loading-hint p-3 text-sm">
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
/* Centered container with max-width, rounded corners, subtle shadow. */
.project-details-root {
  max-width: 48rem;
  margin: 0 auto;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

/* Push the "Edit owners" button to the right inside the flex row. */
.structure-edit-btn {
  margin-left: auto;
}

/* Loading/fallback hint for units — brand green tint. */
.units-loading-hint {
  background: #ecfdf5;
  border-radius: 0.5rem;
  border: 1px solid #a7f3d0;
  color: #065f46;
}

/* Brand green used across the Project Structure section. */
.section-icon {
  color: #059669;
}

/* ── Project hero (header) ─────────────────────────────────────────── */
.project-hero {
  display: grid;
  grid-template-columns: 1.6fr 1fr;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 768px) {
  .project-hero {
    grid-template-columns: 1fr;
  }
}

.project-hero__media {
  aspect-ratio: 16 / 10;
  border-radius: 0.85rem;
  overflow: hidden;
  background: #f3f4f6;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.08);
}

.project-hero__img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  transition: transform 0.3s ease;
}

.project-hero__img:hover {
  transform: scale(1.03);
}

.project-hero__placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #10b981, #047857);
}

.project-hero__placeholder .pi {
  font-size: 3.25rem;
  color: #ffffff;
  opacity: 0.92;
}

.project-hero__info {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.project-hero__statusrow {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  background: #ecfdf5;
  border-left: 4px solid #10b981;
  border-radius: 0.6rem;
}

.project-hero__status-label {
  font-weight: 600;
  font-size: 0.85rem;
  color: #065f46;
}

.project-hero__status-badge {
  padding: 0.25rem 0.8rem;
  background: #059669;
  color: #ffffff;
  font-size: 0.78rem;
  font-weight: 700;
  border-radius: 999px;
}

.project-hero__stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.stat {
  display: flex;
  flex-direction: column;
  padding: 0.75rem 1rem;
  background: #f9fafb;
  border: 1px solid #eef0f2;
  border-radius: 0.6rem;
}

.stat__value {
  font-size: 1.6rem;
  font-weight: 700;
  line-height: 1.1;
  color: #111827;
}

.stat__label {
  margin-top: 0.15rem;
  font-size: 0.72rem;
  color: #6b7280;
}

.project-hero__meta {
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
  margin: 0;
  padding: 0.85rem 1rem;
  background: #f9fafb;
  border-radius: 0.6rem;
}

.meta-row {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 1rem;
}

.meta-row dt {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  margin: 0;
  font-size: 0.78rem;
  font-weight: 600;
  color: #6b7280;
}

.meta-row dt .pi {
  font-size: 0.72rem;
  color: #059669;
}

.meta-row dd {
  margin: 0;
  font-size: 0.82rem;
  color: #111827;
  text-align: right;
}

.meta-row--block {
  flex-direction: column;
  align-items: stretch;
}

.meta-row--block dd {
  margin-top: 0.25rem;
  text-align: left;
  line-height: 1.4;
}

.project-hero__error {
  padding: 0.75rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 0.6rem;
  color: #b91c1c;
  font-size: 0.82rem;
}

.project-hero__error .pi {
  margin-right: 0.4rem;
}

.structure-floors {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* One card per floor. */
.floor-card {
  background: #f9fafb;
  border: 1px solid #eef0f2;
  border-radius: 0.75rem;
  padding: 1rem 1.25rem;
}

.floor-card__head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 0.85rem;
}

.floor-card__name {
  font-weight: 700;
  font-size: 0.95rem;
  color: #1f2937;
}

.floor-card__occ {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.75rem;
  font-weight: 500;
  color: #6b7280;
}

.floor-card__occ .pi {
  font-size: 0.7rem;
  color: #059669;
}

/* Responsive grid: wraps to fill the row width, scales to any unit count. */
.unit-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 0.6rem;
}

.unit-card {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  padding: 0.6rem 0.75rem;
  border: 1px solid #e5e7eb;
  border-radius: 0.6rem;
  background: #ffffff;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.unit-card__top {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.unit-card__top .pi {
  font-size: 0.8rem;
}

.unit-card__no {
  font-weight: 700;
  font-size: 0.9rem;
  color: #111827;
}

.unit-card__status {
  font-size: 0.72rem;
  color: #9ca3af;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Vacant unit — neutral. */
.unit-card--vacant .pi {
  color: #9ca3af;
}

/* Occupied unit — brand green accent. */
.unit-card--occupied {
  border-color: #a7f3d0;
  background: #ecfdf5;
}

.unit-card--occupied .pi {
  color: #059669;
}

.unit-card--occupied .unit-card__status {
  color: #047857;
  font-weight: 500;
}

/* Edit mode: vacant unit ready to receive an owner. */
.unit-card--edit {
  border-style: dashed;
  border-color: #fcd34d;
  background: #fffbeb;
}

.unit-card__editrow {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.unit-card__input {
  flex: 1;
  min-width: 0;
  font-size: 0.75rem;
}

/* Edit mode: occupied unit laid out inline with a clear button. */
.unit-card--editrow-inline {
  flex-direction: row;
  align-items: center;
  gap: 0.4rem;
}

.unit-card__email {
  flex: 1;
  min-width: 0;
  font-size: 0.72rem;
  color: #047857;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

:deep(.unit-card__input.p-inputtext) {
  width: 100%;
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}
</style>
