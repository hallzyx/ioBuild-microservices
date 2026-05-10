<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  onAddDevice: {
    type: Function,
    required: true
  }
});
</script>

<template>
  <div class="mb-4">
    <div class="header-row">
      <h1 class="text-3xl font-extrabold text-gray-900">{{ t('devices.title') }}</h1>
      <pv-button
        :label="t('devices.actions.add')"
        icon="pi pi-plus"
        @click="onAddDevice"
        class="add-device-btn"
      />
    </div>
  </div>
</template>

<style scoped>
/* Header row layout */
.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

/* Estilos para el botón Add Device */
:deep(.add-device-btn) {
  background: #10B981 !important;
  border-color: #10B981 !important;
  color: white !important;
}

:deep(.add-device-btn:hover) {
  background: #059669 !important;
  border-color: #059669 !important;
  color: white !important;
}
</style>
