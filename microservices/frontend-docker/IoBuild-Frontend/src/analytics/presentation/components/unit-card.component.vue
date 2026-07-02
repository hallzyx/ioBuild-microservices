<script setup>
const props = defineProps({
  unit: {
    type: Object,
    required: true
  }
});

const unitLabel = () => {
  if (props.unit.floor && props.unit.roomNumber) {
    return `Floor ${props.unit.floor} · Unit ${props.unit.roomNumber}`;
  }
  return `Unit #${props.unit.unitId}`;
};

const isOccupied = () => props.unit.status === 'Occupied';
</script>

<template>
  <div class="unit-card">
    <div class="unit-header">
      <div class="unit-number">
        <i class="pi pi-home unit-home-icon"></i>
        <span>{{ unitLabel() }}</span>
      </div>
      <span class="connection-status" :class="isOccupied() ? 'status-occupied' : 'status-active'">
        <i :class="['pi', isOccupied() ? 'pi-check-circle' : 'pi-home']"></i>
        {{ isOccupied() ? 'Occupied' : 'Available' }}
      </span>
    </div>

    <div class="unit-info">
      <p class="project-name">
        <i class="pi pi-building unit-info-icon"></i>
        {{ unit.projectName }}
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

.status-occupied {
  color: #16A34A;
  background-color: #D1FAE5;
}

.status-active {
  color: #2563EB;
  background-color: #DBEAFE;
}

.unit-info {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.project-name {
  font-size: 0.875rem;
  color: #6B7280;
  display: flex;
  align-items: center;
  gap: 0.375rem;
}

.unit-home-icon {
  color: #7C3AED;
}

.unit-info-icon {
  font-size: 0.875rem;
}
</style>
