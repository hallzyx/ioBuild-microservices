<script setup>
import { reactive, computed, ref, watch, onMounted } from 'vue';
import { useAnalyticsStore } from '../../../analytics/application/analytics.store.js';
import { DeviceApi } from '../../infrastructure/device-api.js';

/**
 * Dialog for adding a catalog device to an owner's unit (Slice 3 — catalog picker).
 *
 * Flow:
 *  1. Owner picks a device type from the catalog (scope "unit" or "both" only).
 *  2. Owner enters a device name and selects which unit to assign it to.
 *  3. On submit: POST /api/v1/devices with { type, name, location, projectId, unitId }.
 *
 * The two-step custom-type-creation flow (POST /types/custom + POST /devices) is removed.
 * Emits `created` with the new device DTO on success; the parent closes the dialog.
 */

const props = defineProps({
  modelValue: { type: Boolean, default: false },
});

const emit = defineEmits(['update:modelValue', 'created']);

const analyticsStore = useAnalyticsStore();

// ── Catalog types ─────────────────────────────────────────────────────────────

const catalogTypes = ref([]);
const catalogLoading = ref(false);

async function loadCatalogTypes() {
  catalogLoading.value = true;
  try {
    const api = new DeviceApi();
    const all = await api.getDeviceTypes();
    // Filter to types that are valid for unit-level devices
    catalogTypes.value = all.filter(
      (t) => t.scope === 'unit' || t.scope === 'both'
    );
  } catch (err) {
    console.error('[AddCustomDeviceDialog] failed to load catalog types:', err);
    catalogTypes.value = [];
  } finally {
    catalogLoading.value = false;
  }
}

const typeOptions = computed(() =>
  catalogTypes.value.map((t) => ({
    label: t.displayName,
    value: t.code,
  }))
);

// ── Unit options ──────────────────────────────────────────────────────────────

/** Units sourced from the owner dashboard payload (myUnitsDetails). */
const unitOptions = computed(() =>
  (analyticsStore.ownerDashboard?.myUnitsDetails ?? []).map((u) => ({
    label: `Unit ${u.unitId} — ${u.projectName ?? 'Unknown project'}`,
    value: u.unitId,
    projectId: u.projectId ?? null,
  }))
);

// ── Form state ────────────────────────────────────────────────────────────────

const form = reactive({
  selectedTypeCode: null,
  deviceName: '',
  selectedUnitId: null,
});

// ── Visibility ────────────────────────────────────────────────────────────────

const visible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
});

// Reset form when dialog opens; also (re)load catalog types
watch(visible, (isOpen) => {
  if (isOpen) {
    form.selectedTypeCode = null;
    form.deviceName = '';
    form.selectedUnitId = unitOptions.value[0]?.value ?? null;
    errorMessage.value = '';
    loadCatalogTypes();
  }
});

onMounted(() => {
  // Pre-load so the picker is populated quickly on first open
  loadCatalogTypes();
});

// ── Inline errors ─────────────────────────────────────────────────────────────

const errorMessage = ref('');
const submitting = ref(false);

// ── Validation ────────────────────────────────────────────────────────────────

function validate() {
  if (!form.selectedTypeCode) return 'Please select a device type.';
  if (!form.deviceName.trim()) return 'Device name is required.';
  if (form.selectedUnitId == null) return 'Please select a unit.';
  return null;
}

// ── Submit ────────────────────────────────────────────────────────────────────

async function onSubmit() {
  errorMessage.value = '';
  const validationError = validate();
  if (validationError) {
    errorMessage.value = validationError;
    return;
  }

  submitting.value = true;

  try {
    // Derive projectId from the selected unit — never hardcode 1
    const selectedUnit = unitOptions.value.find((u) => u.value === form.selectedUnitId);
    const projectId = selectedUnit?.projectId ?? null;

    if (!projectId) {
      errorMessage.value = 'Could not determine the project for the selected unit. Please reload and try again.';
      return;
    }

    const api = new DeviceApi();
    const newDevice = await api.createDevice({
      name: form.deviceName.trim(),
      type: form.selectedTypeCode,
      location: `Unit ${form.selectedUnitId}`,
      projectId,
      status: 'active',
      unitId: form.selectedUnitId,
    });

    emit('created', newDevice);
    visible.value = false;
  } catch (err) {
    const status = err?.response?.status;
    const serverMessage = err?.response?.data?.error;

    if (status === 409) {
      errorMessage.value = serverMessage ?? 'A device of this type already exists in this unit.';
    } else if (status === 400) {
      errorMessage.value = serverMessage ?? 'Validation error. Please check your selection.';
    } else if (status === 403) {
      errorMessage.value = 'Permission denied. Make sure you own the selected unit.';
    } else {
      errorMessage.value = serverMessage ?? 'An unexpected error occurred. Please try again.';
    }
    console.error('[AddCustomDeviceDialog] submit error:', err);
  } finally {
    submitting.value = false;
  }
}

function onCancel() {
  visible.value = false;
}
</script>

<template>
  <pv-dialog
    v-model:visible="visible"
    modal
    header="Add Device"
    :style="{ width: '480px' }"
    contentClass="add-device-dialog"
    @hide="onCancel"
  >
    <div class="dialog-body">

      <!-- Error banner -->
      <pv-message v-if="errorMessage" severity="error" :closable="false" class="error-banner">
        {{ errorMessage }}
      </pv-message>

      <!-- Device type picker -->
      <div class="field">
        <label class="field-label">Device Type <span class="required">*</span></label>
        <pv-select
          v-model="form.selectedTypeCode"
          :options="typeOptions"
          option-label="label"
          option-value="value"
          :loading="catalogLoading"
          placeholder="Select a device type"
          class="field-input"
        />
        <small v-if="typeOptions.length === 0 && !catalogLoading" class="hint warn">
          No unit-compatible device types available.
        </small>
      </div>

      <!-- Device name (instance label) -->
      <div class="field">
        <label class="field-label">Device Name <span class="required">*</span></label>
        <pv-input-text
          v-model="form.deviceName"
          placeholder="e.g. Living Room AC"
          maxlength="100"
          class="field-input"
        />
      </div>

      <!-- Unit picker -->
      <div class="field">
        <label class="field-label">Assign to Unit <span class="required">*</span></label>
        <pv-select
          v-model="form.selectedUnitId"
          :options="unitOptions"
          option-label="label"
          option-value="value"
          placeholder="Select a unit"
          class="field-input"
        />
        <small v-if="unitOptions.length === 0" class="hint warn">
          No units found. Make sure your owner dashboard has loaded.
        </small>
      </div>

    </div>

    <template #footer>
      <div class="dialog-footer">
        <pv-button label="Cancel" text @click="onCancel" />
        <pv-button
          label="Add Device"
          icon="pi pi-check"
          :loading="submitting"
          :disabled="catalogLoading"
          @click="onSubmit"
        />
      </div>
    </template>
  </pv-dialog>
</template>

<style scoped>
.dialog-body {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  padding: 0.25rem 0;
}

:deep(.add-device-dialog) {
  background: #ffffff !important;
  color: #111827 !important;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.field-label {
  font-weight: 500;
  color: #111827;
  font-size: 0.9rem;
}

.field-input {
  width: 100%;
}

.required {
  color: #ef4444;
}

.hint {
  color: #6b7280;
  font-size: 0.75rem;
}

.hint.warn {
  color: #f59e0b;
}

.error-banner {
  margin-bottom: 0.25rem;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  width: 100%;
}
</style>
