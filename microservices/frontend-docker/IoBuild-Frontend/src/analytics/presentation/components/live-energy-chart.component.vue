<script setup>
import { computed, onMounted, onUnmounted } from 'vue';
import { Line } from 'vue-chartjs';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';
import { useAnalyticsStore } from '../../application/analytics.store.js';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler);

const props = defineProps({
  userId: {
    type: Number,
    required: true
  },
  role: {
    type: String,
    required: true,
    validator: (v) => ['builder', 'owner'].includes(v)
  },
  minutes: {
    type: Number,
    default: 10
  }
});

const analyticsStore = useAnalyticsStore();

onMounted(() => {
  analyticsStore.startLiveEnergyPolling(props.userId, props.role, props.minutes);
});

onUnmounted(() => {
  analyticsStore.stopLiveEnergyPolling();
});

const chartData = computed(() => {
  const data = analyticsStore.liveEnergyData;
  if (!data?.length) return null;

  return {
    labels: data.map(p => new Date(p.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit' })),
    datasets: [{
      label: 'Energy (kWh)',
      data: data.map(p => p.totalEnergyKwh),
      borderColor: '#10B981',
      backgroundColor: 'rgba(16, 185, 129, 0.1)',
      fill: true,
      tension: 0.4,
      pointRadius: 3
    }]
  };
});

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: true,
      position: 'bottom'
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      title: {
        display: true,
        text: 'kWh'
      }
    }
  }
};
</script>

<template>
  <div class="live-energy-chart-inner">
    <div v-if="analyticsStore.liveEnergyLoading && !analyticsStore.liveEnergyData.length" class="no-data">
      Loading telemetry...
    </div>
    <p v-else-if="!chartData" class="no-data">No telemetry data yet</p>
    <Line v-else :data="chartData" :options="chartOptions" />
  </div>
</template>

<style scoped>
.live-energy-chart-inner {
  width: 100%;
  height: 100%;
  position: relative;
}

.no-data {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: #9CA3AF;
  font-size: 0.8125rem;
  white-space: nowrap;
}
</style>
