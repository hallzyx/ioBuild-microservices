<script setup>
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  device: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['edit', 'delete']);

const menu = ref();
const selectedDevice = ref(null);

const menuItems = computed(() => [
  {
    label: t('devices.actions.edit'),
    icon: 'pi pi-pencil',
    command: () => {
      emit('edit', selectedDevice.value);
    }
  },
  {
    label: t('devices.actions.delete'),
    icon: 'pi pi-trash',
    command: () => {
      emit('delete', selectedDevice.value);
    }
  }
]);

const toggleMenu = (event, device) => {
  selectedDevice.value = device;
  menu.value.toggle(event);
};
</script>

<template>
  <div>
    <pv-menu ref="menu" :model="menuItems" :popup="true">
      <template #item="{ item }">
        <div class="p-menu-item-content" @click="item.command">
          <span :class="item.icon" class="mr-2"></span>
          <span>{{ item.label }}</span>
        </div>
      </template>
    </pv-menu>

    <div class="flex gap-2 justify-end">
      <pv-button icon="pi pi-cog" rounded text @click="emit('edit', device)" />
      <pv-button icon="pi pi-trash" rounded text severity="danger" @click="emit('delete', device)" />
    </div>
  </div>
</template>

<style scoped>
/* Estilos para el menú popup */
:deep(.p-menu) {
  background: white !important;
}

:deep(.p-menu .p-menu-list) {
  background: white !important;
  padding: 0.25rem !important;
}

:deep(.p-menu .p-menu-item-content) {
  padding: 0.75rem 1rem !important;
  cursor: pointer !important;
  color: #111827 !important;
  background: white !important;
  border-radius: 0.375rem !important;
  transition: all 0.2s !important;
}

:deep(.p-menu .p-menu-item-content:hover) {
  background: #f3f4f6 !important;
  color: #111827 !important;
}

:deep(.p-menu .p-menu-item-content .pi) {
  color: #6b7280 !important;
}

:deep(.p-menu .p-menu-item-content:hover .pi) {
  color: #111827 !important;
}
</style>
