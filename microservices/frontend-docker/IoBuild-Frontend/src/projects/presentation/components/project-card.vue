<script setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();

defineProps({
  project: { type: Object, required: true },
});
defineEmits(["viewDetails"]);

function handleImageError(event) {
  event.target.src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='32' height='32'%3E%3Crect width='32' height='32' rx='4' fill='%2310B981'/%3E%3Ctext x='16' y='21' text-anchor='middle' fill='white' font-size='14' font-family='sans-serif'%3EP%3C/text%3E%3C/svg%3E";
}
</script>

<template>
  <pv-card class="card-root bg-white text-gray-900 p-1 flex flex-column align-items-center text-center justify-content-center">
    <template #content>
      <div class="card-image-wrap mb-4 overflow-hidden">
        <img
            :src="project.imageUrl"
            :alt="project.name"
            class="card-image"
            @error="handleImageError"
            loading="lazy"
        />
      </div>
      <h3 class="card-name font-semibold text-gray-800 w-full mb-4" :title="project.name">{{ project.name }}</h3>
      <div class="card-meta text-gray-600 mb-1 w-full flex-shrink-0">
        <p class="card-meta__row mb-1"><strong>{{ t("projects.fields.status") }}:</strong> {{ project.statusLabel }}</p>
        <p class="card-meta__row mb-1"><strong>{{ t("projects.fields.occupancy-rate") }}:</strong> {{ project.occupiedUnits }}/{{ project.totalUnits }}</p>
        <p class="card-meta__row mb-1"><strong>{{ t("projects.fields.created-date") }}:</strong> {{ project.createdDate?.slice(0,10) }}</p>
      </div>

      <pv-button
        :label="t('projects.actions.view-details')"
        icon="pi pi-info-circle"
        size="small"
        class="card-btn custom-green-button w-full"
        @click="$emit('viewDetails')"
      />
    </template>
  </pv-card>
</template>

<style scoped>
.card-root {
  border-radius: 0.25rem;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  transition: box-shadow 0.2s ease;
}

.card-root:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

/* Image wrapper: fixed 6rem × 6rem, centered */
.card-image-wrap {
  width: 6rem;
  height: 6rem;
  border-radius: 0.25rem;
  margin: 0 auto;
}

.card-image {
  width: 6rem;
  height: 6rem;
  object-fit: cover;
  display: block;
}

/* Card title: tiny 9px text, single-line truncate */
.card-name {
  font-size: 9px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-bottom: 0.125rem;
}

/* Meta rows: 8px text, tight line-height, single-line truncate */
.card-meta {
  font-size: 8px;
  line-height: 1.25;
}

.card-meta__row {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* View-details button: compact 8px text, tight padding */
:deep(.card-btn.p-button) {
  font-size: 8px !important;
  padding: 0.125rem 0.25rem !important;
}

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
