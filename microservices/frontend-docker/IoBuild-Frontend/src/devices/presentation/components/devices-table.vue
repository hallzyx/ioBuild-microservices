<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  devices: {
    type: Array,
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['edit', 'delete']);

const getTypeLabel = (type) => {
  const key = `devices.types.${type}`;
  const translated = t(key);
  return translated === key ? (type ? type.charAt(0).toUpperCase() + type.slice(1) : '') : translated;
};

const getSeverity = (device) => {
  if (device.isOnline && device.isOnline()) {
    return 'success';
  }
  return 'danger';
};

const getStatusLabel = (device) => {
  if (device.isOnline && device.isOnline()) {
    return t('devices.status.online');
  }
  return t('devices.status.offline');
};

const handleEdit = (device) => {
  emit('edit', device);
};

const handleDelete = (device) => {
  emit('delete', device);
};
</script>

<template>
  <pv-data-table
    :value="devices"
    dataKey="id"
    :loading="loading"
    class="shadow-sm devices-table white-table-force"
    :pt="{
      table: {
        style: 'min-width: 60rem; background: #ffffff !important; color-scheme: light !important;'
      },
      wrapper: {
        style: 'background: #ffffff !important; color-scheme: light !important;'
      }
    }"
    :emptyMessage="t('devices.table.empty')"
  >
    <template #loading>
      <div class="flex items-center gap-2 py-4">
        <pv-progress-spinner style="width:24px;height:24px" strokeWidth="4" />
        <span class="font-medium text-gray-600">{{ t('devices.loading') }}</span>
      </div>
    </template>

    <pv-column field="name" :header="t('devices.table.name')" sortable />
    <pv-column field="type" :header="t('devices.table.type')" sortable>
      <template #body="{ data }">{{ getTypeLabel(data.type) }}</template>
    </pv-column>
    <pv-column field="location" :header="t('devices.table.location')" sortable />
    <pv-column :header="t('devices.table.realtimeStatus')">
      <template #body="{ data }">
        <pv-tag
          :value="getStatusLabel(data)"
          :severity="getSeverity(data)"
        />
      </template>
    </pv-column>
    <pv-column :header="t('devices.table.actions')" style="width:8rem" bodyClass="text-right">
      <template #body="{ data }">
        <div class="flex gap-2 justify-end">
          <pv-button icon="pi pi-cog" rounded text @click="handleEdit(data)" />
          <pv-button icon="pi pi-trash" rounded text severity="danger" @click="handleDelete(data)" />
        </div>
      </template>
    </pv-column>
  </pv-data-table>
</template>

<style scoped>
.devices-table {
  color-scheme: light !important;
  background-color: #ffffff !important;
}

.white-table-force {
  background: #ffffff !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  background: #ffffff !important;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #e5e7eb !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  background: #ffffff !important;
  box-shadow: none !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  background: #ffffff !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  background: #ffffff !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  background: #ffffff !important;
  color-scheme: light only !important;
}

:deep(.devices-table .p-datatable-thead > tr) {
  background: #ffffff !important;
  color-scheme: light only !important;
}

:deep(.devices-table .p-datatable-thead > tr > th) {
  background: #ffffff !important;
  color: #374151 !important;
  font-weight: 600 !important;
  border: none !important;
  border-bottom: 1px solid #e5e7eb !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  background: #ffffff !important;
  color-scheme: light only !important;
}

:deep(.devices-table .p-datatable-tbody > tr) {
  background: #ffffff !important;
  color-scheme: light only !important;
}

:deep(.devices-table .p-datatable-tbody > tr > td) {
  background: #ffffff !important;
  color: #111827 !important;
  border: none !important;
  border-bottom: 1px solid #f3f4f6 !important;
  color-scheme: light only !important;
}

:deep(.devices-table *) {
  color-scheme: light only !important;
}

/* Hover effect */
:deep(.devices-table .p-datatable-tbody > tr:hover) {
  background: #f8fafc !important;
  color-scheme: light only !important;
}

:deep(.devices-table .p-datatable-tbody > tr:hover > td) {
  background: #f8fafc !important;
  color: #111827 !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  font-weight: 600;
  border-radius: 9999px;
  padding: 0.25rem 0.75rem;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  background: #dcfce7 !important;
  color: #111827 !important;
}

:deep(.devices-table) {
  background: #fee2e2 !important;
  color: #111827 !important;
}


:deep(.devices-table) {
  background: rgba(255,255,255,0.9) !important;
  color-scheme: light only !important;
}


:deep(.devices-table) {
  background: transparent !important;
  color-scheme: light only !important;
}

:deep(.devices-table) {
  color: #6b7280 !important;
}

:deep(.devices-table) {
  background: #f3f4f6 !important;
  color: #374151 !important;
}

:deep(.devices-table) {
  color: #dc2626 !important;
}

:deep(.devices-table) {
  background: #fee2e2 !important;
  color: #991b1b !important;
}

:deep(.devices-table),
:deep(.devices-table *) {
  color-scheme: light only !important;
}

:deep(.devices-table ),
:deep(.devices-table ),
:deep(.devices-table ),
:deep(.devices-table ),
:deep(.devices-table ),
:deep(.devices-table ) {
  background: #ffffff !important;
  color-scheme: light only !important;
}
</style>
