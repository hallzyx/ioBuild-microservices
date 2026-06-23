<script setup>
import { ref, computed, watch } from 'vue';
import { useCommandStore } from '../../application/command.store.js';
import { useToast } from 'primevue/usetoast';

const props = defineProps({
  /**
   * Device row from the unit-devices table.
   * Expected shape: { id, deviceName, type }
   * where `type` matches a DeviceCapabilityCatalog code (e.g. 'AirConditioner').
   */
  device: {
    type: Object,
    required: true,
  },
  /**
   * ControllableAttributes array for this device type.
   * Shape: [{ name, type, min, max, unit, enumMembers }]
   * `type` is the catalog value kind: 'number', 'enum', 'boolean'
   * (comparison is case-insensitive to stay resilient to contract casing).
   */
  controllableAttributes: {
    type: Array,
    default: () => [],
  },
});

const commandStore = useCommandStore();
const toast = useToast();

// Fixed dropdown options for boolean attributes — the catalog does not send enumMembers for them.
const BOOLEAN_OPTIONS = [
  { label: 'On', value: true },
  { label: 'Off', value: false },
];

// Normalise the attribute value kind so comparisons are casing-agnostic.
function kind(attr) {
  return (attr?.type ?? '').toLowerCase();
}

// Local reactive values — one entry per controllable attribute
const localValues = ref({});

// Initialise local values whenever attributes change
watch(
  () => props.controllableAttributes,
  (attrs) => {
    const init = {};
    attrs.forEach((attr) => {
      const k = kind(attr);
      if (k === 'number') {
        // Start at min (or 0)
        init[attr.name] = attr.min ?? 0;
      } else if (k === 'boolean') {
        init[attr.name] = false;
      } else if (k === 'enum') {
        init[attr.name] = attr.enumMembers?.[0] ?? '';
      } else {
        init[attr.name] = '';
      }
    });
    localValues.value = init;
  },
  { immediate: true }
);

const hasControls = computed(() => props.controllableAttributes.length > 0);

// The device row may come from the analytics dashboard payload (deviceId) or the
// devices service (id). Accept either so the command targets the right device.
const deviceId = computed(() => props.device?.deviceId ?? props.device?.id);

function enumOptions(attr) {
  if (kind(attr) === 'boolean') return BOOLEAN_OPTIONS;
  return (attr.enumMembers ?? []).map((m) => ({ label: m, value: m }));
}

async function onSend(attr) {
  const value = localValues.value[attr.name];
  const result = await commandStore.sendCommand(deviceId.value, attr.name, value);

  if (result.success) {
    toast.add({
      severity: 'success',
      summary: 'Command sent',
      detail: `${attr.name} set to ${value}${attr.unit ? ' ' + attr.unit : ''}`,
      life: 3000,
    });
  } else {
    toast.add({
      severity: result.status === 403 ? 'warn' : 'error',
      summary: result.status === 403 ? 'Permission denied' : 'Command failed',
      detail: result.message,
      life: 5000,
    });
  }
}
</script>

<template>
  <div v-if="hasControls" class="control-panel">
    <div
      v-for="attr in controllableAttributes"
      :key="attr.name"
      class="control-row"
    >
      <div class="control-meta">
        <span class="control-label">{{ attr.name }}</span>
        <span v-if="attr.unit" class="control-unit">{{ attr.unit }}</span>
      </div>

      <!-- Numeric range: use native range input (no Slider registered in this project) -->
      <template v-if="kind(attr) === 'number'">
        <div class="slider-group">
          <input
            type="range"
            :min="attr.min"
            :max="attr.max"
            :step="1"
            v-model.number="localValues[attr.name]"
            class="native-slider"
          />
          <span class="slider-value">{{ localValues[attr.name] }}</span>
        </div>
      </template>

      <!-- Enum / Boolean: dropdown -->
      <template v-else-if="kind(attr) === 'enum' || kind(attr) === 'boolean'">
        <pv-select
          v-model="localValues[attr.name]"
          :options="enumOptions(attr)"
          option-label="label"
          option-value="value"
          class="control-select"
        />
      </template>

      <pv-button
        label="Send"
        icon="pi pi-send"
        size="small"
        :loading="commandStore.sending"
        @click="onSend(attr)"
        class="send-btn"
      />
    </div>
  </div>
</template>

<style scoped>
.control-panel {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 0.75rem 0;
}

.control-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.control-meta {
  display: flex;
  flex-direction: column;
  min-width: 140px;
}

.control-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: #374151;
}

.control-unit {
  font-size: 0.75rem;
  color: #9CA3AF;
}

.slider-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
}

.native-slider {
  flex: 1;
  accent-color: #8B5CF6;
  cursor: pointer;
}

.slider-value {
  min-width: 2.5rem;
  text-align: right;
  font-size: 0.875rem;
  font-weight: 700;
  color: #111827;
}

.control-select {
  flex: 1;
  min-width: 140px;
}

.send-btn {
  flex-shrink: 0;
}
</style>
