<script setup>
import { reactive, computed, ref, watch } from 'vue';
import { useDeviceStore } from '../../application/device.store.js';
import { useAnalyticsStore } from '../../../analytics/application/analytics.store.js';
import { useIamStore } from '../../../iam/application/iam.store.js';
import { DeviceApi } from '../../infrastructure/device-api.js';

/**
 * Dialog for adding an owner-custom device type and creating a device of that type.
 *
 * Flow:
 *  1. Owner fills in display name, typeCode, and one or more controllable attributes.
 *  2. Owner selects which of their units to assign the device to.
 *  3. On submit: POST /api/v1/devices/types/custom → then POST /api/v1/devices (with unitId).
 *
 * Emits `created` with the new device DTO on success; the parent closes the dialog.
 */

const props = defineProps({
  modelValue: { type: Boolean, default: false },
});

const emit = defineEmits(['update:modelValue', 'created']);

const deviceStore = useDeviceStore();
const analyticsStore = useAnalyticsStore();
const iamStore = useIamStore();

// ── Owner context ─────────────────────────────────────────────────────────────

const ownerId = computed(() => iamStore.currentUser?.id ?? null);

/** Units sourced from the owner dashboard payload (myUnitsDetails). */
const unitOptions = computed(() =>
  (analyticsStore.ownerDashboard?.myUnitsDetails ?? []).map((u) => ({
    label: `Unit ${u.unitId} — ${u.projectName ?? 'Unknown project'}`,
    value: u.unitId,
  }))
);

// ── Form state ────────────────────────────────────────────────────────────────

const KIND_OPTIONS = [
  { label: 'Number', value: 'number' },
  { label: 'Boolean', value: 'boolean' },
  { label: 'Enum', value: 'enum' },
];

function emptyAttribute() {
  return { name: '', kind: 'number', min: null, max: null, unit: '', enumMembersRaw: '' };
}

const form = reactive({
  displayName: '',
  typeCode: '',
  selectedUnitId: null,
  deviceName: '',
  attributes: [emptyAttribute()],
});

// ── Visibility ────────────────────────────────────────────────────────────────

const visible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
});

// Reset form when dialog opens
watch(visible, (isOpen) => {
  if (isOpen) {
    form.displayName = '';
    form.typeCode = '';
    form.selectedUnitId = unitOptions.value[0]?.value ?? null;
    form.deviceName = '';
    form.attributes = [emptyAttribute()];
    errorMessage.value = '';
  }
});

// ── Attribute helpers ─────────────────────────────────────────────────────────

function addAttribute() {
  form.attributes.push(emptyAttribute());
}

function removeAttribute(index) {
  if (form.attributes.length > 1) {
    form.attributes.splice(index, 1);
  }
}

// ── Inline errors ─────────────────────────────────────────────────────────────

const errorMessage = ref('');
const submitting = ref(false);

// ── Validation ────────────────────────────────────────────────────────────────

function validate() {
  if (!form.displayName.trim()) return 'Display name is required.';
  if (form.displayName.trim().length > 100) return 'Display name must be 100 characters or fewer.';
  if (!form.typeCode.trim()) return 'Type code is required.';
  if (!form.deviceName.trim()) return 'Device name is required.';
  if (form.selectedUnitId == null) return 'Please select a unit.';
  if (form.attributes.length === 0) return 'At least one attribute is required.';

  for (let i = 0; i < form.attributes.length; i++) {
    const a = form.attributes[i];
    if (!a.name.trim()) return `Attribute ${i + 1}: name is required.`;
    if (a.kind === 'number') {
      if (a.min == null || a.max == null) return `Attribute "${a.name}": min and max are required for number type.`;
      if (!a.unit.trim()) return `Attribute "${a.name}": unit is required for number type.`;
    }
    if (a.kind === 'enum') {
      const members = a.enumMembersRaw.split(',').map((s) => s.trim()).filter(Boolean);
      if (members.length < 2) return `Attribute "${a.name}": enum type requires at least two comma-separated members.`;
    }
  }

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

  // Build the attributes array matching CreateCustomTypeResource shape
  const attributes = form.attributes.map((a) => {
    const attr = { name: a.name.trim(), type: a.kind };
    if (a.kind === 'number') {
      attr.min = Number(a.min);
      attr.max = Number(a.max);
      attr.unit = a.unit.trim();
    }
    if (a.kind === 'enum') {
      attr.enumMembers = a.enumMembersRaw.split(',').map((s) => s.trim()).filter(Boolean);
    }
    return attr;
  });

  try {
    // Step 1: create the custom type
    await deviceStore.createCustomType(
      {
        typeCode: form.typeCode.trim(),
        displayName: form.displayName.trim(),
        attributes,
      },
      ownerId.value
    );

    // Step 2: create a device of that type, assigned to the chosen unit
    const api = new DeviceApi();
    const newDevice = await api.createDevice({
      name: form.deviceName.trim(),
      type: form.typeCode.trim(),
      location: `Unit ${form.selectedUnitId}`,
      macAddress: '',
      projectId: 1,
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
      errorMessage.value = serverMessage ?? 'Validation error. Please check your input.';
    } else if (status === 403) {
      errorMessage.value = 'Permission denied. Only owners may create custom device types.';
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
    header="Add Custom Device"
    :style="{ width: '560px' }"
    contentClass="add-custom-device-dialog"
    @hide="onCancel"
  >
    <div class="dialog-body flex flex-column gap-4">

      <!-- Error banner -->
      <pv-message v-if="errorMessage" severity="error" :closable="false" class="mb-2">
        {{ errorMessage }}
      </pv-message>

      <!-- Display name -->
      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">Display Name <span class="required">*</span></label>
        <pv-input-text
          v-model="form.displayName"
          placeholder="e.g. CO2 Monitor"
          maxlength="100"
          class="text-input"
        />
      </div>

      <!-- Type code -->
      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">Type Code <span class="required">*</span></label>
        <pv-input-text
          v-model="form.typeCode"
          placeholder="e.g. CO2Sensor (no spaces)"
          class="text-input"
        />
        <small class="hint">Unique identifier for this device type. Used internally.</small>
      </div>

      <!-- Device name (instance name) -->
      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">Device Name <span class="required">*</span></label>
        <pv-input-text
          v-model="form.deviceName"
          placeholder="e.g. My CO2 Sensor"
          class="text-input"
        />
      </div>

      <!-- Unit picker -->
      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">Assign to Unit <span class="required">*</span></label>
        <pv-select
          v-model="form.selectedUnitId"
          :options="unitOptions"
          option-label="label"
          option-value="value"
          placeholder="Select a unit"
          class="select-input"
        />
        <small v-if="unitOptions.length === 0" class="hint warn">
          No units found. Make sure your owner dashboard has loaded.
        </small>
      </div>

      <!-- Attributes section -->
      <div class="attributes-section">
        <div class="attributes-header">
          <span class="font-medium text-gray-800">Controllable Attributes <span class="required">*</span></span>
          <pv-button
            label="Add Attribute"
            icon="pi pi-plus"
            size="small"
            text
            @click="addAttribute"
          />
        </div>

        <div
          v-for="(attr, index) in form.attributes"
          :key="index"
          class="attribute-row"
        >
          <div class="attr-fields">
            <!-- Attribute name -->
            <pv-input-text
              v-model="attr.name"
              placeholder="Attribute name"
              class="attr-name-input"
            />

            <!-- Kind selector -->
            <pv-select
              v-model="attr.kind"
              :options="KIND_OPTIONS"
              option-label="label"
              option-value="value"
              class="attr-kind-select"
            />

            <!-- Remove button -->
            <pv-button
              icon="pi pi-trash"
              severity="danger"
              text
              size="small"
              :disabled="form.attributes.length <= 1"
              @click="removeAttribute(index)"
              class="remove-btn"
            />
          </div>

          <!-- Number-specific inputs -->
          <div v-if="attr.kind === 'number'" class="attr-extra-fields">
            <div class="flex flex-column gap-1">
              <label class="attr-sub-label">Min</label>
              <pv-input-text v-model.number="attr.min" type="number" placeholder="0" class="attr-small-input" />
            </div>
            <div class="flex flex-column gap-1">
              <label class="attr-sub-label">Max</label>
              <pv-input-text v-model.number="attr.max" type="number" placeholder="100" class="attr-small-input" />
            </div>
            <div class="flex flex-column gap-1">
              <label class="attr-sub-label">Unit</label>
              <pv-input-text v-model="attr.unit" placeholder="e.g. ppm" class="attr-small-input" />
            </div>
          </div>

          <!-- Enum-specific inputs -->
          <div v-if="attr.kind === 'enum'" class="attr-extra-fields">
            <div class="flex flex-column gap-1 w-full">
              <label class="attr-sub-label">Members (comma-separated, min 2)</label>
              <pv-input-text
                v-model="attr.enumMembersRaw"
                placeholder="e.g. low, medium, high"
                class="text-input"
              />
            </div>
          </div>

          <!-- Boolean: no extra inputs needed -->
        </div>
      </div>
    </div>

    <template #footer>
      <div class="flex justify-end gap-2 w-full">
        <pv-button label="Cancel" text @click="onCancel" />
        <pv-button
          label="Create Device"
          icon="pi pi-check"
          :loading="submitting"
          @click="onSubmit"
        />
      </div>
    </template>
  </pv-dialog>
</template>

<style scoped>
.flex-column { display: flex; flex-direction: column; }

:deep(.add-custom-device-dialog) {
  background: #ffffff !important;
  color: #111827 !important;
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

.attributes-section {
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
}

.attributes-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.attribute-row {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.75rem 0;
  border-top: 1px solid #f3f4f6;
}

.attribute-row:first-of-type {
  border-top: none;
  padding-top: 0;
}

.attr-fields {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.attr-name-input {
  flex: 2;
}

.attr-kind-select {
  flex: 1;
}

.remove-btn {
  flex-shrink: 0;
}

.attr-extra-fields {
  display: flex;
  gap: 0.75rem;
  padding-left: 0.25rem;
}

.attr-small-input {
  width: 90px;
}

.attr-sub-label {
  font-size: 0.75rem;
  color: #6b7280;
}

.select-input {
  width: 100%;
}
</style>
