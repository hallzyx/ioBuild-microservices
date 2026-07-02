<script setup>
import { onMounted, onActivated, onUnmounted, computed, watch } from 'vue';
import { useAnalyticsStore } from '../../application/analytics.store.js';
import { useIamStore } from '../../../iam/application/iam.store.js';
import BuilderDashboard from '../components/builder-dashboard.component.vue';
import OwnerDashboard from '../components/owner-dashboard.component.vue';

const analyticsStore = useAnalyticsStore();
const iamStore = useIamStore();

const currentUser = computed(() => iamStore.currentUser);
const userRole = computed(() => currentUser.value?.role?.toLowerCase() || 'owner');
// No fallback to 1 — null means "not authenticated yet"; guard in loadDashboard prevents bogus calls.
const userId = computed(() => currentUser.value?.id ?? null);

let retryTimer = null;

async function loadDashboard() {
  // Wait until the IAM store is hydrated; watch below re-triggers when userId becomes available.
  if (!userId.value) return;

  if (userRole.value === 'builder') {
    await analyticsStore.fetchBuilderDashboard(userId.value);

    // Retry when projects exist but units/devices are still 0 — covers read-model eventual-consistency lag.
    const dash = analyticsStore.builderDashboard;
    if (dash && dash.activeProjectsCount > 0 && (dash.totalUnits === 0 || dash.totalDevices === 0)) {
      retryTimer = setTimeout(async () => {
        retryTimer = null;
        await analyticsStore.fetchBuilderDashboard(userId.value);
      }, 2500);
    }
  } else if (userRole.value === 'owner') {
    await analyticsStore.fetchOwnerDashboard(userId.value);

    // Retry until myUnitsCount > 0 to handle Analytics read-model propagation lag.
    // Up to 4 attempts at 1.5 s intervals (~6 s total).
    let ownerRetries = 4;
    const scheduleOwnerRetry = () => {
      if (ownerRetries-- <= 0) return;
      retryTimer = setTimeout(async () => {
        retryTimer = null;
        await analyticsStore.fetchOwnerDashboard(userId.value);
        if ((analyticsStore.ownerDashboard?.myUnitsCount ?? 1) === 0) {
          scheduleOwnerRetry();
        }
      }, 1500);
    };
    const ownerDash = analyticsStore.ownerDashboard;
    if (ownerDash && ownerDash.myUnitsCount === 0) {
      scheduleOwnerRetry();
    }
  }
}

// onMounted fires on every fresh navigation (component is not keep-alive here).
onMounted(loadDashboard);

// onActivated fires only when the component IS wrapped in <keep-alive>.
// Adding it here is defensive: ensures a fresh fetch if the router ever enables keep-alive.
onActivated(loadDashboard);

// Re-fetch when userId hydrates after mount (covers the timing gap where the IAM store
// loads from localStorage asynchronously or after a page refresh with a cold store).
watch(userId, (newId, oldId) => {
  if (newId && !oldId) {
    loadDashboard();
  }
});

onUnmounted(() => {
  if (retryTimer !== null) {
    clearTimeout(retryTimer);
    retryTimer = null;
  }
});
</script>

<template>
  <div class="analytics-view">
    <div v-if="analyticsStore.loading" class="loading-container">
      <pv-progress-spinner />
      <p class="mt-4 loading-text">Loading dashboard...</p>
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
        <p class="empty-text">No dashboard data available</p>
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

.loading-text {
  color: #4B5563;
  margin-top: 1rem;
}

.empty-text {
  color: #6B7280;
  font-size: 0.875rem;
}
</style>
