<script setup>
import { onMounted, computed, ref } from 'vue';
import { useDeviceStore } from '../../application/device.store.js';
import { useIamStore } from '../../../iam/application/iam.store.js';
import { useAnalyticsStore } from '../../../analytics/application/analytics.store.js';
import { useI18n } from 'vue-i18n';
import DeviceEditDialog from '../components/device-edit-dialog.vue';
import DeviceListHeader from '../components/device-list-header.vue';
import DevicesTable from '../components/devices-table.vue';
import MyUnitDevices from '../components/my-unit-devices.vue';
import AddCustomDeviceDialog from '../components/AddCustomDeviceDialog.vue';
import { useToast } from 'primevue/usetoast';
import { useConfirm } from 'primevue/useconfirm';

const { t } = useI18n();
const deviceStore = useDeviceStore();
const iamStore = useIamStore();
const analyticsStore = useAnalyticsStore();
const toast = useToast();
const confirm = useConfirm();

// Owners get their own owner-scoped unit-device panel (with control affordances);
// builders get the all-tenant device CRUD management table.
const isOwner = computed(() => (iamStore.currentUser?.role ?? '').toLowerCase() === 'owner');

const devices = computed(() => deviceStore.devices);
const loading = computed(() => deviceStore.loading);
const error = computed(() => deviceStore.error);

const showEditDialog = ref(false);
const editingDevice = ref(null);
const isNewDialog = ref(false);

// Owner: custom device creation dialog
const showCustomDeviceDialog = ref(false);

const openCustomDeviceDialog = () => {
  showCustomDeviceDialog.value = true;
};

const handleCustomDeviceCreated = async (newDevice) => {
  toast.add({
    severity: 'success',
    summary: 'Device added',
    detail: `Device "${newDevice?.name ?? ''}" added successfully.`,
    life: 3000,
  });

  // The owner device list (MyUnitDevices) is sourced from the analytics projection
  // (ownerDashboard.deviceHealthStatus), which updates asynchronously after the
  // DeviceCreatedEvent propagates from the devices service. Re-fetch the owner dashboard,
  // retrying briefly until the new device appears, so the list reflects the add without a
  // manual page refresh. As soon as the projection has caught up the loop stops early.
  const ownerId = iamStore.currentUser?.id;
  if (ownerId == null) return;

  const newDeviceVisible = () =>
    (analyticsStore.ownerDashboard?.deviceHealthStatus ?? [])
      .some((d) => d.deviceId === newDevice?.id);

  for (let attempt = 0; attempt < 6; attempt++) {
    await analyticsStore.fetchOwnerDashboard(ownerId);
    if (newDevice?.id == null || newDeviceVisible()) break;
    await new Promise((resolve) => setTimeout(resolve, 800));
  }
};

const addDevice = () => {
  isNewDialog.value = true;
  editingDevice.value = { id: null, name: '', type: '', location: '', status: 'Online', projectId: null, macAddress: '' };
  showEditDialog.value = true;
};

const handleEditDevice = (device) => {
  isNewDialog.value = false;
  deviceStore.setSelectedDevice(device);
  editingDevice.value = { ...device };
  showEditDialog.value = true;
};

const handleDeleteDevice = (device) => {
  confirm.require({
    message: t('devices.messages.deleteConfirmMessage', { name: device.name }),
    header: t('devices.messages.deleteConfirmTitle'),
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('devices.actions.cancel'),
    acceptLabel: t('devices.actions.delete'),
    rejectClass: 'p-button-secondary p-button-outlined',
    acceptClass: 'p-button-danger',
    accept: async () => {
      try {
        const success = await deviceStore.deleteDevice(device.id);
        if (success) {
          toast.add({
            severity: 'success',
            summary: t('devices.messages.deleteSuccess'),
            life: 2500
          });
        } else {
          throw new Error('delete-failed');
        }
      } catch (error) {
        toast.add({
          severity: 'error',
          summary: t('devices.messages.deleteError'),
          life: 3000
        });
      }
    }
  });
};

const handleSave = async (payload) => {
  try {
    if (isNewDialog.value || !payload.id) {
      const created = await deviceStore.createDevice({
        name: payload.name,
        type: payload.type,
        location: payload.location,
        status: payload.status,
        projectId: payload.projectId || 1,
        macAddress: payload.macAddress || ''
      });
      if (!created) throw new Error('create-failed');
      toast.add({ severity: 'success', summary: t('devices.messages.createSuccess'), life: 2500 });
    } else {
      const updated = await deviceStore.updateDevice(payload);
      if (!updated) throw new Error('update-failed');
      toast.add({ severity: 'success', summary: t('devices.messages.updateSuccess'), life: 2500 });
    }
    // Cerrar el diálogo y limpiar el estado
    showEditDialog.value = false;
    editingDevice.value = null;
    isNewDialog.value = false;
  } catch (e) {
    const key = isNewDialog.value ? 'devices.messages.createError' : 'devices.messages.updateError';
    toast.add({ severity: 'error', summary: t(key), life: 3000 });
  }
};

onMounted(async () => {
  // Skip the all-tenant device fetch for owners — they use the owner-scoped panel.
  if (!isOwner.value) {
    await deviceStore.fetchDevices();
  }
});
</script>

<template>
  <div class="device-management-view">
    <pv-toast />

    <!-- Owner: owner-scoped unit devices with control panel -->
    <template v-if="isOwner">
      <div class="owner-actions mb-4">
        <pv-button
          label="Add Device"
          icon="pi pi-plus"
          @click="openCustomDeviceDialog"
        />
      </div>
      <MyUnitDevices />
      <AddCustomDeviceDialog
        v-model="showCustomDeviceDialog"
        @created="handleCustomDeviceCreated"
      />
    </template>

    <!-- Builder: all-tenant device CRUD management -->
    <template v-else>
      <DeviceListHeader :on-add-device="addDevice" />

      <pv-message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</pv-message>

      <DevicesTable
        :devices="devices"
        :loading="loading"
        @edit="handleEditDevice"
        @delete="handleDeleteDevice"
      />

      <DeviceEditDialog
        v-model="showEditDialog"
        :device="editingDevice"
        :isNew="isNewDialog"
        @save="handleSave"
        @cancel="showEditDialog = false"
      />
    </template>
  </div>
</template>

<style scoped>
.device-management-view {
  background: white;
  padding: 1.5rem 2rem;
  min-height: 100vh;
}

.owner-actions {
  display: flex;
  justify-content: flex-end;
}

.mb-4 {
  margin-bottom: 1rem;
}
</style>