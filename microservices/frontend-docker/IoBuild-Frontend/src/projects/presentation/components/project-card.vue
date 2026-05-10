<script setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();

defineProps({
  project: { type: Object, required: true },
});
defineEmits(["viewDetails"]);

function handleImageError(event) {
  event.target.src = "https://via.placeholder.com/32/10B981/ffffff?text=P"; // tamaño acorde al ícono
}
</script>

<template>
  <pv-card class="bg-white text-gray-900 p-1 rounded-sm shadow hover:shadow-md transition flex flex-col items-center text-center justify-content-center">
    <template #content>
      <div class="w-24 h-24 mb-4 overflow-hidden rounded-sm mx-auto">
        <img
            :src="project.imageUrl"
            :alt="project.name"
            class="w-24 h-24 object-cover"
            style="max-width: 24rem; max-height: 24rem;"
            @error="handleImageError"
            loading="lazy"
        />
      </div>
      <h3 class="text-[9px] font-semibold mb-0.5 text-gray-800 truncate w-full mb-4" :title="project.name">{{ project.name }}</h3>
      <div class="text-[8px] text-gray-600 leading-tight space-y-0.5 mb-1 w-full flex-shrink-0">
        <p class="truncate mb-1"><strong>{{ t("projects.fields.status") }}:</strong> {{ project.status }}</p>
        <p class="truncate mb-1"><strong>{{ t("projects.fields.occupancy-rate") }}:</strong> {{ project.occupiedUnits }}/{{ project.totalUnits }}</p>
        <p class="truncate mb-1"><strong>{{ t("projects.fields.created-date") }}:</strong> {{ project.createdDate?.slice(0,10) }}</p>
      </div>

      <pv-button
        :label="t('projects.actions.view-details')"
        icon="pi pi-info-circle"
        size="small"
        class="custom-green-button !py-0.5 !px-1 !text-[8px] w-full"
        @click="$emit('viewDetails')"
      />
    </template>
  </pv-card>
</template>

<style scoped>
:deep(.custom-green-button) {
  background-color: #10B981 !important;
  border-color: #10B981 !important;
  color: white !important;
}
:deep(.custom-green-button:hover) {
  background-color: #059669 !important;
  border-color: #059669 !important;
}
:deep(.custom-green-button:focus) {
  box-shadow: 0 0 0 0.15rem rgba(16, 185, 129, 0.35) !important;
}
</style>