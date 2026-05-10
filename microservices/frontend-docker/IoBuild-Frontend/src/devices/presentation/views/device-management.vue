<script setup>
import { onMounted, computed, ref } from 'vue';
import { useDeviceStore } from '../../application/device.store.js';
import { useI18n } from 'vue-i18n';
import DeviceEditDialog from '../components/device-edit-dialog.vue';
import DeviceListHeader from '../components/device-list-header.vue';
import DevicesTable from '../components/devices-table.vue';
import { useToast } from 'primevue/usetoast';
import { useConfirm } from 'primevue/useconfirm';

const { t } = useI18n();
const deviceStore = useDeviceStore();
const toast = useToast();
const confirm = useConfirm();

const devices = computed(() => deviceStore.devices);
const loading = computed(() => deviceStore.loading);
const error = computed(() => deviceStore.error);

const showEditDialog = ref(false);
const editingDevice = ref(null);
const isNewDialog = ref(false);

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
  await deviceStore.fetchDevices();
});
</script>

<template>
  <div class="p-4">
    <pv-toast />

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
  </div>
</template>

<style scoped>
.p-4 {
  background: white !important;
}
</style>