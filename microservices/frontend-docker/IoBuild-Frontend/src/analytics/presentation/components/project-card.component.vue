<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  project: {
    type: Object,
    required: true
  }
});

const translateStatus = (status) => {
  return t(`analytics.projectStatus.${status}`, status);
};

const statusColors = {
  'OnGoing': 'bg-green-100 text-green-800',
  'Active': 'bg-green-100 text-green-800',
  'Construction': 'bg-orange-100 text-orange-800',
  'Planned': 'bg-blue-100 text-blue-800',
  'Completed': 'bg-gray-100 text-gray-800'
};

const getStatusColor = (status) => {
  return statusColors[status] || 'bg-gray-100 text-gray-800';
};
</script>

<template>
  <div class="project-card">
    <div class="project-header">
      <div class="project-info">
        <h4 class="project-name">{{ project.name }}</h4>
        <p class="project-location">
          <i class="pi pi-map-marker text-sm"></i>
          {{ project.location }}
        </p>
      </div>
      <span class="project-status" :class="getStatusColor(project.status)">
        {{ translateStatus(project.status) }}
      </span>
    </div>
    
    <div class="project-stats">
      <div class="stat-item">
        <span class="stat-icon-small">
          <i class="pi pi-building"></i>
        </span>
        <div class="stat-details">
          <span class="stat-value-small">{{ project.occupiedUnits }}/{{ project.totalUnits }}</span>
          <span class="stat-label-small">{{ $t('analytics.common.units') }}</span>
        </div>
      </div>
      
      <div class="stat-item">
        <span class="stat-icon-small">
          <i class="pi pi-box"></i>
        </span>
        <div class="stat-details">
          <span class="stat-value-small">{{ project.deviceCount }}</span>
          <span class="stat-label-small">{{ $t('analytics.common.devices') }}</span>
        </div>
      </div>
    </div>
    
    <div class="progress-section">
      <div class="progress-header">
        <span class="progress-label">{{ $t('analytics.common.occupancy') }}</span>
        <span class="progress-value">{{ project.occupancyRate.toFixed(1) }}%</span>
      </div>
      <div class="progress-bar">
        <div class="progress-fill" :style="{ width: `${project.occupancyRate}%` }"></div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.project-card {
  background: white;
  border-radius: 0.75rem;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s, box-shadow 0.2s;
}

.project-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.project-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
  gap: 1rem;
}

.project-info {
  flex: 1;
  min-width: 0;
}

.project-name {
  font-size: 1.125rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 0.25rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.project-location {
  font-size: 0.875rem;
  color: #6B7280;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.project-status {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  white-space: nowrap;
}

.project-stats {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #E5E7EB;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.stat-icon-small {
  width: 2rem;
  height: 2rem;
  border-radius: 0.5rem;
  background: #F3F4F6;
  color: #6B7280;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-details {
  display: flex;
  flex-direction: column;
}

.stat-value-small {
  font-size: 0.875rem;
  font-weight: 700;
  color: #111827;
  line-height: 1;
}

.stat-label-small {
  font-size: 0.75rem;
  color: #9CA3AF;
  margin-top: 0.125rem;
}

.progress-section {
  margin-top: 1rem;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.progress-label {
  font-size: 0.875rem;
  color: #6B7280;
  font-weight: 500;
}

.progress-value {
  font-size: 0.875rem;
  font-weight: 700;
  color: #111827;
}

.progress-bar {
  width: 100%;
  height: 0.5rem;
  background: #E5E7EB;
  border-radius: 9999px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #3B82F6 0%, #2563EB 100%);
  border-radius: 9999px;
  transition: width 0.3s ease;
}
</style>
