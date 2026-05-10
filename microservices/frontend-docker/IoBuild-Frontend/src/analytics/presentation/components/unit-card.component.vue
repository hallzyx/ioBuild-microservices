<script setup>
const props = defineProps({
  unit: {
    type: Object,
    required: true
  }
});

const getStatusColor = (status) => {
  return status === 'Connected' 
    ? 'text-green-600 bg-green-100' 
    : 'text-red-600 bg-red-100';
};
</script>

<template>
  <div class="unit-card">
    <div class="unit-header">
      <div class="unit-number">
        <i class="pi pi-home text-purple-600"></i>
        <span>{{ unit.unitNumber }}</span>
      </div>
      <span class="connection-status" :class="getStatusColor(unit.connectionStatus)">
        <i :class="['pi', unit.connectionStatus === 'Connected' ? 'pi-check-circle' : 'pi-times-circle']"></i>
        {{ unit.connectionStatus }}
      </span>
    </div>
    
    <div class="unit-info">
      <p class="project-name">
        <i class="pi pi-building text-sm"></i>
        {{ unit.projectName }}
      </p>
      <p class="device-count">
        <i class="pi pi-box text-sm"></i>
        {{ unit.activeDevices }} Active Devices
      </p>
    </div>
  </div>
</template>

<style scoped>
.unit-card {
  background: white;
  border-radius: 0.75rem;
  padding: 1.25rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 2px solid #F3F4F6;
  transition: all 0.2s;
}

.unit-card:hover {
  border-color: #9333EA;
  box-shadow: 0 4px 12px rgba(147, 51, 234, 0.15);
}

.unit-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #F3F4F6;
}

.unit-number {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.125rem;
  font-weight: 700;
  color: #111827;
}

.connection-status {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.375rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.unit-info {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.project-name,
.device-count {
  font-size: 0.875rem;
  color: #6B7280;
  display: flex;
  align-items: center;
  gap: 0.375rem;
}

.device-count {
  font-weight: 600;
  color: #111827;
}
</style>
