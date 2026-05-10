<script setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { Line, Bar } from 'vue-chartjs';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, BarElement, Title, Tooltip, Legend, Filler } from 'chart.js';
import StatCard from './stat-card.component.vue';
import UnitCard from './unit-card.component.vue';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, Title, Tooltip, Legend, Filler);

const { t } = useI18n();

const props = defineProps({
  dashboard: {
    type: Object,
    required: true
  }
});

// Energy consumption chart (last 30 days)
const energyConsumptionChartData = computed(() => {
  if (!props.dashboard?.dailyEnergyConsumption?.length) return null;
  
  const data = props.dashboard.dailyEnergyConsumption.slice(-30);
  return {
    labels: data.map(point => new Date(point.timestamp).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })),
    datasets: [{
      label: t('analytics.owner.charts.dailyEnergyLabel'),
      data: data.map(point => point.value),
      borderColor: '#8B5CF6',
      backgroundColor: 'rgba(139, 92, 246, 0.1)',
      fill: true,
      tension: 0.4
    }]
  };
});

// Temperature comfort chart (last 7 days)
const temperatureComfortChartData = computed(() => {
  if (!props.dashboard?.temperatureHistory?.length) return null;
  
  const data = props.dashboard.temperatureHistory.slice(-7);
  return {
    labels: data.map(point => new Date(point.timestamp).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })),
    datasets: [{
      label: t('analytics.owner.charts.temperatureLabel'),
      data: data.map(point => point.value),
      borderColor: '#F59E0B',
      backgroundColor: 'rgba(245, 158, 11, 0.2)',
      fill: true,
      tension: 0.4
    }]
  };
});

// Water usage chart (last 7 days)
const waterUsageChartData = computed(() => {
  if (!props.dashboard?.waterUsageWeekly?.length) return null;
  
  return {
    labels: props.dashboard.waterUsageWeekly.map(point => 
      new Date(point.timestamp).toLocaleDateString('en-US', { weekday: 'short' })
    ),
    datasets: [{
      label: t('analytics.owner.charts.waterLabel'),
      data: props.dashboard.waterUsageWeekly.map(point => point.value),
      backgroundColor: '#06B6D4',
      borderRadius: 6
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
      beginAtZero: true
    }
  }
};

const getHealthColor = (health) => {
  if (health >= 80) return 'bg-green-500';
  if (health >= 50) return 'bg-yellow-500';
  return 'bg-red-500';
};
</script>

<template>
  <div class="owner-dashboard">
    <div class="dashboard-header">
      <h2 class="dashboard-title">{{ $t('analytics.owner.title') }}</h2>
      <p class="dashboard-subtitle">{{ $t('analytics.owner.subtitle') }}</p>
    </div>
    
    <!-- Hero Stats -->
    <div class="stats-grid">
      <StatCard
        :title="$t('analytics.owner.stats.myUnits')"
        :value="dashboard.myUnitsCount"
        icon="pi-home"
        icon-bg-color="bg-purple-100"
        icon-text-color="text-purple-600"
      />
      <StatCard
        :title="$t('analytics.owner.stats.myDevices')"
        :value="dashboard.totalDevices"
        icon="pi-box"
        icon-bg-color="bg-blue-100"
        icon-text-color="text-blue-600"
        :subtitle="`${dashboard.onlineDevices} ${$t('analytics.owner.stats.online')}`"
      />
      <StatCard
        :title="$t('analytics.owner.stats.energyThisMonth')"
        :value="`${dashboard.energyThisMonth.toFixed(1)} kWh`"
        icon="pi-bolt"
        icon-bg-color="bg-yellow-100"
        icon-text-color="text-yellow-600"
      />
      <StatCard
        :title="$t('analytics.owner.stats.temperatureAvg')"
        :value="`${dashboard.temperatureAvg.toFixed(1)}°C`"
        icon="pi-thermometer"
        icon-bg-color="bg-orange-100"
        icon-text-color="text-orange-600"
      />
      <StatCard
        :title="$t('analytics.owner.stats.alerts')"
        :value="dashboard.alertsCount"
        icon="pi-bell"
        icon-bg-color="bg-red-100"
        icon-text-color="text-red-600"
      />
    </div>
    
    <!-- Charts Grid -->
    <div class="charts-grid">
      <div class="chart-container large">
        <h3 class="chart-title">{{ $t('analytics.owner.charts.energyConsumption') }}</h3>
        <div class="chart-wrapper">
          <Line v-if="energyConsumptionChartData" :data="energyConsumptionChartData" :options="chartOptions" />
          <p v-else class="no-data">{{ $t('analytics.owner.noData.energyConsumption') }}</p>
        </div>
      </div>
      
      <div class="chart-container">
        <h3 class="chart-title">{{ $t('analytics.owner.charts.temperatureComfort') }}</h3>
        <div class="chart-wrapper">
          <Line v-if="temperatureComfortChartData" :data="temperatureComfortChartData" :options="chartOptions" />
          <p v-else class="no-data">{{ $t('analytics.owner.noData.temperature') }}</p>
        </div>
      </div>
      
      <div class="chart-container">
        <h3 class="chart-title">{{ $t('analytics.owner.charts.waterUsage') }}</h3>
        <div class="chart-wrapper">
          <Bar v-if="waterUsageChartData" :data="waterUsageChartData" :options="chartOptions" />
          <p v-else class="no-data">{{ $t('analytics.owner.noData.waterUsage') }}</p>
        </div>
      </div>
    </div>
    
    <!-- Device Health Status -->
    <div class="section">
      <h3 class="section-title">{{ $t('analytics.owner.sections.deviceHealth') }}</h3>
      <div class="device-health-grid">
        <div 
          v-for="device in dashboard.deviceHealthStatus" 
          :key="device.deviceId"
          class="device-health-card"
        >
          <div class="device-health-header">
            <div class="device-info">
              <i class="pi pi-box text-purple-600"></i>
              <span class="device-name">{{ device.deviceName }}</span>
            </div>
            <span 
              class="device-status" 
              :class="device.status === 'Online' ? 'status-online' : 'status-offline'"
            >
              {{ device.status === 'Online' ? $t('analytics.owner.device.online') : $t('analytics.owner.device.offline') }}
            </span>
          </div>
          <div class="device-health-body">
            <div class="progress-bar-container">
              <div class="progress-bar-bg">
                <div 
                  class="progress-bar-fill" 
                  :class="getHealthColor(device.healthPercentage)"
                  :style="{ width: `${device.healthPercentage}%` }"
                ></div>
              </div>
              <span class="progress-label">{{ device.healthPercentage.toFixed(0) }}%</span>
            </div>
            <p class="device-type">{{ device.type }}</p>
          </div>
        </div>
      </div>
      <p v-if="!dashboard.deviceHealthStatus?.length" class="no-data">{{ $t('analytics.owner.noData.deviceHealth') }}</p>
    </div>
    
    <!-- My Units Overview -->
    <div class="section">
      <h3 class="section-title">{{ $t('analytics.owner.sections.myUnitsOverview') }}</h3>
      <div class="units-grid">
        <UnitCard
          v-for="unit in dashboard.myUnitsDetails"
          :key="unit.unitId"
          :unit="unit"
        />
      </div>
      <p v-if="!dashboard.myUnitsDetails?.length" class="no-data">{{ $t('analytics.owner.noData.units') }}</p>
    </div>
  </div>
</template>

<style scoped>
.owner-dashboard {
  padding: 1.5rem;
  max-width: 1400px;
  margin: 0 auto;
}

.dashboard-header {
  margin-bottom: 2rem;
}

.dashboard-title {
  font-size: 2rem;
  font-weight: 800;
  color: #111827;
  margin-bottom: 0.5rem;
}

.dashboard-subtitle {
  font-size: 1rem;
  color: #6B7280;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.25rem;
  margin-bottom: 2rem;
}

.charts-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.chart-container {
  background: white;
  border-radius: 0.75rem;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.chart-container.large {
  grid-column: 1 / -1;
}

.chart-title {
  font-size: 1.125rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 1rem;
}

.chart-wrapper {
  height: 300px;
  position: relative;
}

.section {
  background: white;
  border-radius: 0.75rem;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  margin-bottom: 1.5rem;
}

.section-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 1.5rem;
}

.device-health-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1rem;
}

.device-health-card {
  background: #F9FAFB;
  border-radius: 0.75rem;
  padding: 1.25rem;
  border: 2px solid #F3F4F6;
  transition: all 0.2s;
}

.device-health-card:hover {
  border-color: #8B5CF6;
  box-shadow: 0 4px 12px rgba(139, 92, 246, 0.15);
}

.device-health-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.device-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.device-name {
  font-size: 0.875rem;
  font-weight: 700;
  color: #111827;
}

.device-status {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.status-online {
  background: #D1FAE5;
  color: #065F46;
}

.status-offline {
  background: #FEE2E2;
  color: #991B1B;
}

.device-health-body {
  margin-top: 0.75rem;
}

.progress-bar-container {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
}

.progress-bar-bg {
  flex: 1;
  height: 0.75rem;
  background: #E5E7EB;
  border-radius: 9999px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  border-radius: 9999px;
  transition: width 0.3s ease;
}

.progress-label {
  font-size: 0.875rem;
  font-weight: 700;
  color: #111827;
  min-width: 3rem;
  text-align: right;
}

.device-type {
  font-size: 0.75rem;
  color: #6B7280;
  margin-top: 0.5rem;
}

.units-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.25rem;
}

.no-data {
  text-align: center;
  color: #9CA3AF;
  padding: 2rem;
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .owner-dashboard {
    padding: 1rem;
  }
  
  .charts-grid {
    grid-template-columns: 1fr;
  }
  
  .chart-container.large {
    grid-column: 1;
  }
  
  .device-health-grid,
  .units-grid {
    grid-template-columns: 1fr;
  }
}
</style>
