<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  client: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['edit', 'delete']);
</script>

<template>
  <pv-menu ref="menu" :model="menuItems" :popup="true">
    <template #item="{ item }">
      <div class="p-menu-item-content" @click="item.command">
        <span :class="item.icon" class="mr-2"></span>
        <span>{{ item.label }}</span>
      </div>
    </template>
  </pv-menu>
</template>

<script>
export default {
  computed: {
    menuItems() {
      return [
        {
          label: this.t('clients.actions.edit'),
          icon: 'pi pi-pencil',
          command: () => {
            this.$emit('edit', this.client);
          }
        },
        {
          label: this.t('clients.actions.delete'),
          icon: 'pi pi-trash',
          command: () => {
            this.$emit('delete', this.client);
          }
        }
      ];
    }
  }
};
</script>

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

