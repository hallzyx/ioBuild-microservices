<script setup>
import { ref, computed, watch } from 'vue';
import { useToast } from 'primevue/usetoast';
import useProjectStore from '../../application/project.store.js';

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
const toast = useToast();

const localVisible = ref(props.visible);
const floors = ref(1);
const unitsPerFloor = ref(1);
const showOwnerAssignment = ref(false);
const submitting = ref(false);

watch(() => props.visible, (val) => {
    localVisible.value = val;
    if (val) {
        floors.value = 1;
        unitsPerFloor.value = 1;
        showOwnerAssignment.value = false;
        ownerEmails.value = {};
    }
});

watch(localVisible, (val) => {
    emit('update:visible', val);
});

// ownerEmails keyed by "floor-roomNumber" e.g. "1-01"
const ownerEmails = ref({});

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
    for (let f = 1; f <= floors.value; f++) {
        for (let u = 1; u <= unitsPerFloor.value; u++) {
            const room = String(u).padStart(2, '0');
            const key = `${f}-${room}`;
            if (ownerEmails.value[key] !== undefined) {
                next[key] = ownerEmails.value[key];
            }
        }
    }
    ownerEmails.value = next;
});

function ownerKey(floor, roomNumber) {
    return `${floor}-${roomNumber}`;
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
    return {
        floors: floors.value,
        unitsPerFloor: unitsPerFloor.value,
        ownerEmails: ownerAssignments
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
        :style="{ width: '640px', maxWidth: '95vw' }"
        class="define-structure-dialog"
    >
        <div class="space-y-5">
            <!-- Floors & Units per floor -->
            <div class="grid grid-cols-2 gap-4">
                <div>
                    <label class="block mb-2 font-semibold text-gray-700">
                        <i class="pi pi-building text-emerald-600 mr-2"></i>
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
                <div>
                    <label class="block mb-2 font-semibold text-gray-700">
                        <i class="pi pi-th-large text-emerald-600 mr-2"></i>
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
            <div class="p-3 bg-emerald-50 rounded-lg border-l-4 border-emerald-500 text-sm text-emerald-800">
                <i class="pi pi-info-circle mr-2"></i>
                This will create <strong>{{ (floors || 0) * (unitsPerFloor || 0) }}</strong> unit(s):
                {{ floors || 0 }} floor(s) × {{ unitsPerFloor || 0 }} unit(s) per floor.
            </div>

            <!-- Optional owner assignment -->
            <div>
                <div
                    class="flex items-center gap-2 cursor-pointer select-none text-emerald-700 font-semibold text-sm"
                    @click="showOwnerAssignment = !showOwnerAssignment"
                >
                    <i :class="showOwnerAssignment ? 'pi pi-chevron-down' : 'pi pi-chevron-right'"></i>
                    Assign owner emails (optional)
                </div>

                <div v-if="showOwnerAssignment" class="mt-3 space-y-4 max-h-64 overflow-y-auto pr-1">
                    <div v-for="row in unitGrid" :key="row.floor">
                        <p class="font-semibold text-gray-600 text-sm mb-2">Floor {{ row.floor }}</p>
                        <div class="grid grid-cols-1 gap-2">
                            <div
                                v-for="unit in row.units"
                                :key="ownerKey(unit.floor, unit.roomNumber)"
                                class="flex items-center gap-3"
                            >
                                <span class="text-xs font-mono text-gray-500 w-16 shrink-0">
                                    F{{ unit.floor }}-{{ unit.roomNumber }}
                                </span>
                                <pv-input-text
                                    v-model="ownerEmails[ownerKey(unit.floor, unit.roomNumber)]"
                                    class="w-full"
                                    type="email"
                                    placeholder="owner@example.com"
                                />
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
:deep(.p-dialog) {
    background: white !important;
}

:deep(.p-dialog .p-dialog-header) {
    background: white !important;
    color: #111827 !important;
}

:deep(.p-dialog .p-dialog-content) {
    background: white !important;
    color: #111827 !important;
}

:deep(.p-dialog .p-dialog-footer) {
    background: white !important;
}

:deep(.p-inputtext) {
    background: white !important;
    color: #111827 !important;
    border-color: #d1d5db !important;
}

:deep(.p-inputnumber-input) {
    background: white !important;
    color: #111827 !important;
}
</style>
