<script setup>
import { ref, computed, watch } from 'vue';
import { useToast } from 'primevue/usetoast';
import useProjectStore from '../../application/project.store.js';
import { useDeviceStore } from '../../../devices/application/device.store.js';

const props = defineProps({
    visible: {
        type: Boolean,
        required: true
    },
    projectId: {
        type: [Number, String],
        required: true
    }
});

const emit = defineEmits(['update:visible', 'structure-defined']);

const store = useProjectStore();
const deviceStore = useDeviceStore();
const toast = useToast();

const localVisible = ref(props.visible);
const floors = ref(1);
const unitsPerFloor = ref(1);
// UI-only presentation state: which floor accordion panels are open.
const expandedFloors = ref(new Set([1]));
const submitting = ref(false);

// Per-floor device-type selections — keyed by floor number (integer)
const deviceTypesByFloor = ref({});

// Per-unit device package selections — keyed by "floor-roomNumber" e.g. "1-01"
const unitDevicePackages = ref({});

watch(() => props.visible, (val) => {
    localVisible.value = val;
    if (val) {
        floors.value = 1;
        unitsPerFloor.value = 1;
        expandedFloors.value = new Set([1]);
        ownerEmails.value = {};
        deviceTypesByFloor.value = {};
        unitDevicePackages.value = {};
        deviceStore.loadDeviceTypes();
    }
});

watch(localVisible, (val) => {
    emit('update:visible', val);
});

// ownerEmails keyed by "floor-roomNumber" e.g. "1-01"
const ownerEmails = ref({});

// Catalog types scoped to floors: scope "floor" or "both" (hides unit-only types).
const floorDeviceTypes = computed(() =>
    deviceStore.deviceTypes.filter(t => t.scope === 'floor' || t.scope === 'both')
);

// Catalog types scoped to units: scope "unit" or "both" (hides floor-only types).
const unitDeviceTypes = computed(() =>
    deviceStore.deviceTypes.filter(t => t.scope === 'unit' || t.scope === 'both')
);

// Recompute grid whenever floors/unitsPerFloor change; clear stale keys
const unitGrid = computed(() => {
    const grid = [];
    for (let f = 1; f <= floors.value; f++) {
        const units = [];
        for (let u = 1; u <= unitsPerFloor.value; u++) {
            const roomNumber = String(u).padStart(2, '0');
            units.push({ floor: f, roomNumber });
        }
        grid.push({ floor: f, units });
    }
    return grid;
});

watch([floors, unitsPerFloor], () => {
    const next = {};
    const nextDeviceTypes = {};
    const nextUnitPackages = {};
    for (let f = 1; f <= floors.value; f++) {
        // Preserve device-type selections for floors still in range
        if (deviceTypesByFloor.value[f] !== undefined) {
            nextDeviceTypes[f] = deviceTypesByFloor.value[f];
        }
        for (let u = 1; u <= unitsPerFloor.value; u++) {
            const room = String(u).padStart(2, '0');
            const key = `${f}-${room}`;
            if (ownerEmails.value[key] !== undefined) {
                next[key] = ownerEmails.value[key];
            }
            if (unitDevicePackages.value[key] !== undefined) {
                nextUnitPackages[key] = unitDevicePackages.value[key];
            }
        }
    }
    ownerEmails.value = next;
    deviceTypesByFloor.value = nextDeviceTypes;
    unitDevicePackages.value = nextUnitPackages;
});

function ownerKey(floor, roomNumber) {
    return `${floor}-${roomNumber}`;
}

// Accordion open/close — presentation only, does not touch the payload.
function toggleFloor(floor) {
    const next = new Set(expandedFloors.value);
    next.has(floor) ? next.delete(floor) : next.add(floor);
    expandedFloors.value = next;
}

function isFloorExpanded(floor) {
    return expandedFloors.value.has(floor);
}

// Count of floor-wide device types selected for a floor (header summary).
function floorDeviceCount(floor) {
    return deviceTypesByFloor.value[floor]?.length ?? 0;
}

function buildPayload() {
    const ownerAssignments = [];
    for (const [key, email] of Object.entries(ownerEmails.value)) {
        if (email && email.trim()) {
            const [floorStr, roomNumber] = key.split('-');
            ownerAssignments.push({
                floor: parseInt(floorStr),
                roomNumber,
                email: email.trim()
            });
        }
    }

    // Only include floors where the builder explicitly selected device types.
    // Sending [] was interpreted by the backend as "use legacy defaults", which
    // created default devices on every floor even when the builder made no selection,
    // causing phantom device inflation (N floors × default count).
    const deviceTypesPerFloor = [];
    for (let f = 1; f <= floors.value; f++) {
        const types = deviceTypesByFloor.value[f];
        if (types && types.length > 0) {
            deviceTypesPerFloor.push({ floor: f, deviceTypes: types });
        }
    }

    // Unit device packages — only include units with at least one type selected
    const unitPackages = [];
    for (const [key, deviceTypes] of Object.entries(unitDevicePackages.value)) {
        if (deviceTypes && deviceTypes.length > 0) {
            const [floorStr, roomNumber] = key.split('-');
            unitPackages.push({
                floor: parseInt(floorStr),
                roomNumber,
                deviceTypes
            });
        }
    }

    return {
        floors: floors.value,
        unitsPerFloor: unitsPerFloor.value,
        ownerEmails: ownerAssignments,
        deviceTypesPerFloor,
        unitDevicePackages: unitPackages
    };
}

async function handleSubmit() {
    if (floors.value < 1 || unitsPerFloor.value < 1) {
        toast.add({
            severity: 'warn',
            summary: 'Validation error',
            detail: 'Floors and units per floor must be at least 1.',
            life: 4000
        });
        return;
    }

    submitting.value = true;
    try {
        const payload = buildPayload();
        await store.defineProjectStructure(props.projectId, payload);
        toast.add({
            severity: 'success',
            summary: 'Structure defined',
            detail: `${floors.value} floor(s) × ${unitsPerFloor.value} unit(s) created successfully.`,
            life: 4000
        });
        localVisible.value = false;
        emit('structure-defined');
    } catch (error) {
        const status = error?.response?.status;
        if (status === 409) {
            toast.add({
                severity: 'warn',
                summary: 'Structure already defined',
                detail: 'This project already has a structure. You cannot redefine it.',
                life: 5000
            });
        } else if (status === 403) {
            toast.add({
                severity: 'error',
                summary: 'Access denied',
                detail: 'Only builders can define project structure.',
                life: 5000
            });
        } else if (status === 422) {
            toast.add({
                severity: 'error',
                summary: 'Validation error',
                detail: 'Invalid structure data. Floors and units per floor must be ≥ 1.',
                life: 5000
            });
        } else {
            toast.add({
                severity: 'error',
                summary: 'Error',
                detail: 'An unexpected error occurred. Please try again.',
                life: 5000
            });
        }
    } finally {
        submitting.value = false;
    }
}

function handleCancel() {
    localVisible.value = false;
}
</script>

<template>
    <pv-dialog
        v-model:visible="localVisible"
        modal
        header="Define Project Structure"
        :style="{ width: '880px', maxWidth: '96vw', maxHeight: '90vh' }"
        class="define-structure-dialog"
    >
        <div class="ds-body">
            <!-- Floors & Units per floor -->
            <div class="ds-grid2">
                <div class="ds-field">
                    <label class="ds-label">
                        <i class="pi pi-building"></i>
                        Floors
                    </label>
                    <pv-input-number
                        v-model="floors"
                        :min="1"
                        class="w-full"
                        placeholder="1"
                        showButtons
                        :allowEmpty="false"
                    />
                </div>
                <div class="ds-field">
                    <label class="ds-label">
                        <i class="pi pi-th-large"></i>
                        Units per floor
                    </label>
                    <pv-input-number
                        v-model="unitsPerFloor"
                        :min="1"
                        class="w-full"
                        placeholder="1"
                        showButtons
                        :allowEmpty="false"
                    />
                </div>
            </div>

            <!-- Summary -->
            <div class="ds-summary">
                <i class="pi pi-info-circle"></i>
                <span>
                    This will create <strong>{{ (floors || 0) * (unitsPerFloor || 0) }}</strong> unit(s):
                    {{ floors || 0 }} floor(s) × {{ unitsPerFloor || 0 }} unit(s) per floor.
                </span>
            </div>

            <!-- Per-floor configuration (accordion: one panel per floor) -->
            <div v-if="deviceStore.deviceTypes.length > 0">
                <p class="ds-section-title">
                    <i class="pi pi-sitemap"></i>
                    Configure each floor
                </p>

                <div class="ds-accordion">
                    <div
                        v-for="row in unitGrid"
                        :key="row.floor"
                        class="floor-panel"
                    >
                        <!-- Floor header (toggle) -->
                        <button
                            type="button"
                            class="floor-panel__head"
                            @click="toggleFloor(row.floor)"
                        >
                            <i
                                class="pi floor-panel__chevron"
                                :class="isFloorExpanded(row.floor) ? 'pi-chevron-down' : 'pi-chevron-right'"
                            ></i>
                            <span class="floor-panel__name">Floor {{ row.floor }}</span>
                            <span class="floor-panel__meta">
                                <span>{{ row.units.length }} unit{{ row.units.length === 1 ? '' : 's' }}</span>
                                <span v-if="floorDeviceCount(row.floor) > 0" class="floor-panel__badge">
                                    <i class="pi pi-wifi"></i>
                                    {{ floorDeviceCount(row.floor) }}
                                </span>
                            </span>
                        </button>

                        <!-- Floor body -->
                        <div v-if="isFloorExpanded(row.floor)" class="floor-panel__body">
                            <!-- Floor-wide IoT devices -->
                            <div class="ds-field">
                                <label class="ds-sublabel">
                                    <i class="pi pi-wifi"></i>
                                    Floor-wide IoT devices
                                </label>
                                <pv-multi-select
                                    v-model="deviceTypesByFloor[row.floor]"
                                    :options="floorDeviceTypes"
                                    option-label="displayName"
                                    option-value="code"
                                    placeholder="Default devices (3)"
                                    class="w-full"
                                    display="chip"
                                />
                            </div>

                            <!-- Per-unit configuration -->
                            <div>
                                <p class="ds-units-title">Units</p>
                                <div class="ds-units">
                                    <div
                                        v-for="unit in row.units"
                                        :key="ownerKey(unit.floor, unit.roomNumber)"
                                        class="unit-block"
                                    >
                                        <span class="unit-block__no">Unit {{ unit.roomNumber }}</span>
                                        <pv-input-text
                                            v-model="ownerEmails[ownerKey(unit.floor, unit.roomNumber)]"
                                            class="w-full"
                                            type="email"
                                            placeholder="Owner email (optional)"
                                        />
                                        <pv-multi-select
                                            v-model="unitDevicePackages[ownerKey(unit.floor, unit.roomNumber)]"
                                            :options="unitDeviceTypes"
                                            option-label="displayName"
                                            option-value="code"
                                            placeholder="Unit devices (optional)"
                                            class="w-full"
                                            display="chip"
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <template #footer>
            <pv-button
                label="Cancel"
                icon="pi pi-times"
                @click="handleCancel"
                severity="secondary"
                outlined
            />
            <pv-button
                label="Define Structure"
                icon="pi pi-check"
                @click="handleSubmit"
                :loading="submitting"
                severity="success"
            />
        </template>
    </pv-dialog>
</template>

<style scoped>
/* ── Dialog shell (PrimeVue overrides) ─────────────────────────────── */
:deep(.p-dialog) { background: #ffffff !important; }
/* Robust height cap: header + footer stay pinned, only the body scrolls.
   Guarantees the footer (Define Structure button) is always on screen,
   no matter how many floors/units are configured.
   Uses the generic .p-dialog selector — same pattern as the background
   rules above that are known to apply; the custom-class selector did not match. */
:deep(.p-dialog) {
    display: flex;
    flex-direction: column;
    max-height: 90vh;
}
:deep(.p-dialog .p-dialog-content) {
    flex: 1 1 auto;
    min-height: 0;
    overflow-y: auto;
}
:deep(.p-dialog .p-dialog-header),
:deep(.p-dialog .p-dialog-footer) {
    flex-shrink: 0;
}
:deep(.p-dialog .p-dialog-header),
:deep(.p-dialog .p-dialog-content),
:deep(.p-dialog .p-dialog-footer) {
    background: #ffffff !important;
    color: #111827 !important;
}
:deep(.p-inputtext) {
    background: #ffffff !important;
    color: #111827 !important;
    border-color: #d1d5db !important;
}
:deep(.p-inputnumber-input) {
    background: #ffffff !important;
    color: #111827 !important;
}
/* Chips wrap instead of overflowing horizontally. */
:deep(.p-multiselect) { width: 100%; }
:deep(.p-multiselect-label) {
    white-space: normal;
    display: flex;
    flex-wrap: wrap;
    gap: 0.25rem;
}

/* ── Content layout ────────────────────────────────────────────────── */
.ds-body {
    display: flex;
    flex-direction: column;
    gap: 1.25rem;
    padding-top: 0.75rem;
    padding-bottom: 0.25rem;
}

.ds-grid2 {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
}

.ds-field { display: flex; flex-direction: column; }

.ds-label {
    display: inline-flex;
    align-items: center;
    gap: 0.45rem;
    margin-bottom: 0.5rem;
    font-weight: 600;
    color: #374151;
}

.ds-label .pi { color: #059669; }

/* Summary callout. */
.ds-summary {
    display: flex;
    align-items: flex-start;
    gap: 0.5rem;
    padding: 0.75rem 1rem;
    background: #ecfdf5;
    border-left: 4px solid #10b981;
    border-radius: 0.55rem;
    font-size: 0.85rem;
    color: #065f46;
}

.ds-summary .pi { margin-top: 0.1rem; color: #059669; }

.ds-section-title {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin: 0 0 0.75rem;
    font-size: 0.9rem;
    font-weight: 600;
    color: #374151;
}

.ds-section-title .pi { color: #059669; }

/* ── Accordion ─────────────────────────────────────────────────────── */
.ds-accordion {
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
}

.floor-panel {
    border: 1px solid #e5e7eb;
    border-radius: 0.7rem;
    overflow: hidden;
}

.floor-panel__head {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    width: 100%;
    padding: 0.7rem 1rem;
    background: #f9fafb;
    border: none;
    cursor: pointer;
    text-align: left;
    transition: background 0.15s ease;
}

.floor-panel__head:hover { background: #f3f4f6; }

.floor-panel__chevron {
    font-size: 0.8rem;
    color: #059669;
}

.floor-panel__name {
    font-weight: 600;
    color: #1f2937;
}

.floor-panel__meta {
    display: inline-flex;
    align-items: center;
    gap: 0.75rem;
    margin-left: auto;
    font-size: 0.75rem;
    color: #6b7280;
}

.floor-panel__badge {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    font-weight: 600;
    color: #047857;
}

.floor-panel__badge .pi { font-size: 0.65rem; }

.floor-panel__body {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;
    border-top: 1px solid #f1f3f5;
}

.ds-sublabel {
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    margin-bottom: 0.4rem;
    font-size: 0.75rem;
    font-weight: 600;
    color: #4b5563;
}

.ds-sublabel .pi { color: #059669; }

.ds-units-title {
    margin: 0 0 0.5rem;
    font-size: 0.7rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: #9ca3af;
}

.ds-units {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
    gap: 0.6rem;
}

.unit-block {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    padding: 0.65rem 0.75rem;
    background: #f9fafb;
    border-radius: 0.55rem;
}

.unit-block__no {
    font-size: 0.75rem;
    font-weight: 600;
    color: #374151;
}
</style>
