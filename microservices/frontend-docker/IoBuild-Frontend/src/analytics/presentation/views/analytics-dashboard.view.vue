<script setup>
import { onMounted, computed } from 'vue';
import { useAnalyticsStore } from '../../application/analytics.store.js';
import BuilderDashboard from '../components/builder-dashboard.component.vue';
import OwnerDashboard from '../components/owner-dashboard.component.vue';

const analyticsStore = useAnalyticsStore();

// Get user from localStorage (this should come from your auth store)
const currentUser = computed(() => {
  const user = localStorage.getItem('currentUser');
  return user ? JSON.parse(user) : null;
});

const userRole = computed(() => currentUser.value?.role || 'owner');
const userId = computed(() => currentUser.value?.id || 1);

onMounted(async () => {
  if (userRole.value === 'builder') {
    await analyticsStore.fetchBuilderDashboard(userId.value);
  } else if (userRole.value === 'owner') {
    await analyticsStore.fetchOwnerDashboard(userId.value);
  }
});
</script>

<template>
  <div class="analytics-view">
    <div v-if="analyticsStore.loading" class="loading-container">
      <pv-progress-spinner />
      <p class="mt-4 text-gray-600">Loading dashboard...</p>
    </div>

    <div v-else-if="analyticsStore.errors.length" class="error-container">
      <pv-message severity="error" :closable="false">
        Failed to load dashboard data. Please try again later.
      </pv-message>
    </div>

    <div v-else>
      <BuilderDashboard 
        v-if="userRole === 'builder' && analyticsStore.builderDashboard"
        :dashboard="analyticsStore.builderDashboard"
      />
      
      <OwnerDashboard 
        v-else-if="userRole === 'owner' && analyticsStore.ownerDashboard"
        :dashboard="analyticsStore.ownerDashboard"
      />

      <div v-else class="empty-state">
        <p class="text-gray-500">No dashboard data available</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.analytics-view {
  min-height: 100vh;
  background-color: #F9FAFB;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 50vh;
}

.error-container {
  padding: 2rem;
  max-width: 600px;
  margin: 2rem auto;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 50vh;
}
</style>
