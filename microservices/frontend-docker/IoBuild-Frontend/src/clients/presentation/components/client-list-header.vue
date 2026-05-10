<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  onAddClient: {
    type: Function,
    required: true
  }
});
</script>

<template>
  <div class="mb-4">
    <div class="header-row">
      <h1 class="text-3xl font-extrabold text-gray-900">{{ t('clients.title') }}</h1>
      <pv-button
        :label="t('clients.actions.add')"
        icon="pi pi-plus"
        @click="onAddClient"
        class="add-client-btn"
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

/* Estilos para el botón Add Client */
:deep(.add-client-btn) {
  background: #10B981 !important;
  border-color: #10B981 !important;
  color: white !important;
}

:deep(.add-client-btn:hover) {
  background: #059669 !important;
  border-color: #059669 !important;
  color: white !important;
}
</style>

