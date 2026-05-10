<script setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { Line, Bar, Doughnut } from 'vue-chartjs';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, BarElement, ArcElement, Title, Tooltip, Legend, Filler } from 'chart.js';
import StatCard from './stat-card.component.vue';
import ProjectCard from './project-card.component.vue';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, ArcElement, Title, Tooltip, Legend, Filler);

const { t } = useI18n();

const props = defineProps({
  dashboard: {
    type: Object,
    required: true
  }
});

// Chart configurations
const hourlyEnergyChartData = computed(() => {
  if (!props.dashboard?.hourlyEnergyData?.length) return null;
  
  const data = props.dashboard.hourlyEnergyData.slice(-24);
  return {
    labels: data.map(point => new Date(point.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })),
    datasets: [{
      label: t('analytics.builder.charts.energyLabel'),
      data: data.map(point => point.value),
      borderColor: '#3B82F6',
      backgroundColor: 'rgba(59, 130, 246, 0.1)',
      fill: true,
      tension: 0.4
    }]
  };
});

const monthlyOccupancyChartData = computed(() => {
  if (!props.dashboard?.monthlyOccupancy?.length) return null;
  
  return {
    labels: props.dashboard.monthlyOccupancy.map(m => m.month),
    datasets: [{
      label: t('analytics.builder.charts.occupancyLabel'),
      data: props.dashboard.monthlyOccupancy.map(m => m.occupancyRate),
      backgroundColor: '#10B981',
      borderRadius: 6
    }]
  };
});

const temperatureTrendChartData = computed(() => {
  if (!props.dashboard?.temperatureHistory?.length) return null;
  
  const data = props.dashboard.temperatureHistory.slice(-7);
  return {
    labels: data.map(point => new Date(point.timestamp).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })),
    datasets: [{
      label: t('analytics.builder.charts.temperatureLabel'),
      data: data.map(point => point.value),
      borderColor: '#F59E0B',
      backgroundColor: 'rgba(245, 158, 11, 0.2)',
      fill: true,
      tension: 0.4
    }]
  };
});

const deviceTypeChartData = computed(() => {
  if (!props.dashboard?.devicesByType || Object.keys(props.dashboard.devicesByType).length === 0) return null;
  
  const colors = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899', '#14B8A6'];
  return {
    labels: Object.keys(props.dashboard.devicesByType).map(type => translateDeviceType(type)),
    datasets: [{
      data: Object.values(props.dashboard.devicesByType),
      backgroundColor: colors.slice(0, Object.keys(props.dashboard.devicesByType).length),
      borderWidth: 2,
      borderColor: '#fff'
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

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'right'
    }
  }
};

// Device type icons
const deviceTypeIcons = {
  'Temperature': 'pi-thermometer',
  'Humidity': 'pi-cloud',
  'Energy': 'pi-bolt',
  'Water': 'pi-inbox',
  'Security': 'pi-shield',
  'Construction': 'pi-wrench',
  'Access Control': 'pi-lock',
  'HVAC': 'pi-sync',
  'Lighting': 'pi-sun'
};

const getDeviceIcon = (type) => deviceTypeIcons[type] || 'pi-box';

const translateDeviceType = (type) => {
  return t(`analytics.deviceTypes.${type}`, type);
};
</script>

<template>
  <div class="builder-dashboard">
    <div class="dashboard-header">
      <h2 class="dashboard-title">{{ $t('analytics.builder.title') }}</h2>
      <p class="dashboard-subtitle">{{ $t('analytics.builder.subtitle') }}</p>
    </div>
    
    <!-- Hero Stats -->
    <div class="stats-grid">
      <StatCard
        :title="$t('analytics.builder.stats.activeProjects')"
        :value="dashboard.activeProjectsCount"
        icon="pi-briefcase"
        icon-bg-color="bg-blue-100"
        icon-text-color="text-blue-600"
      />
      <StatCard
        :title="$t('analytics.builder.stats.connectedDevices')"
        :value="dashboard.totalDevices"
        icon="pi-box"
        icon-bg-color="bg-green-100"
        icon-text-color="text-green-600"
        :subtitle="`${dashboard.onlineDevices} ${$t('analytics.builder.stats.online')}`"
      />
      <StatCard
        :title="$t('analytics.builder.stats.occupiedUnits')"
        :value="`${dashboard.occupiedUnits}/${dashboard.totalUnits}`"
        icon="pi-building"
        icon-bg-color="bg-purple-100"
        icon-text-color="text-purple-600"
        :subtitle="`${dashboard.occupancyRate.toFixed(1)}% ${$t('analytics.builder.stats.occupancy')}`"
      />
      <StatCard
        :title="$t('analytics.builder.stats.totalUnits')"
        :value="dashboard.totalUnits"
        icon="pi-home"
        icon-bg-color="bg-orange-100"
        icon-text-color="text-orange-600"
      />
      <StatCard
        :title="$t('analytics.builder.stats.alerts')"
        :value="dashboard.alertsCount"
        icon="pi-exclamation-triangle"
        icon-bg-color="bg-red-100"
        icon-text-color="text-red-600"
      />
      <StatCard
        :title="$t('analytics.builder.stats.energyEfficiency')"
        :value="`${dashboard.energyEfficiencyAvg.toFixed(1)} kWh`"
        icon="pi-chart-line"
        icon-bg-color="bg-teal-100"
        icon-text-color="text-teal-600"
        :subtitle="$t('analytics.builder.stats.averageConsumption')"
      />
    </div>
    
    <!-- Charts Grid -->
    <div class="charts-grid">
      <div class="chart-container">
        <h3 class="chart-title">{{ $t('analytics.builder.charts.hourlyEnergy') }}</h3>
        <div class="chart-wrapper">
          <Line v-if="hourlyEnergyChartData" :data="hourlyEnergyChartData" :options="chartOptions" />
          <p v-else class="no-data">{{ $t('analytics.builder.noData.hourlyEnergy') }}</p>
        </div>
      </div>
      
      <div class="chart-container">
        <h3 class="chart-title">{{ $t('analytics.builder.charts.monthlyOccupancy') }}</h3>
        <div class="chart-wrapper">
          <Bar v-if="monthlyOccupancyChartData" :data="monthlyOccupancyChartData" :options="chartOptions" />
          <p v-else class="no-data">{{ $t('analytics.builder.noData.occupancy') }}</p>
        </div>
      </div>
      
      <div class="chart-container">
        <h3 class="chart-title">{{ $t('analytics.builder.charts.temperatureTrends') }}</h3>
        <div class="chart-wrapper">
          <Line v-if="temperatureTrendChartData" :data="temperatureTrendChartData" :options="chartOptions" />
          <p v-else class="no-data">{{ $t('analytics.builder.noData.temperature') }}</p>
        </div>
      </div>
      
      <div class="chart-container">
        <h3 class="chart-title">{{ $t('analytics.builder.charts.deviceDistribution') }}</h3>
        <div class="chart-wrapper">
          <Doughnut v-if="deviceTypeChartData" :data="deviceTypeChartData" :options="doughnutOptions" />
          <p v-else class="no-data">{{ $t('analytics.builder.noData.device') }}</p>
        </div>
      </div>
    </div>
    
    <!-- Devices Distribution -->
    <div class="section">
      <h3 class="section-title">{{ $t('analytics.builder.sections.devicesDistribution') }}</h3>
      <div class="devices-grid">
        <div 
          v-for="(count, type) in dashboard.devicesByType" 
          :key="type"
          class="device-type-card"
        >
          <div class="device-type-icon">
            <i :class="['pi', getDeviceIcon(type), 'text-2xl']"></i>
          </div>
          <div class="device-type-info">
            <p class="device-type-name">{{ translateDeviceType(type) }}</p>
            <p class="device-type-count">{{ count }} {{ $t('analytics.builder.sections.devices') }}</p>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Projects Overview -->
    <div class="section">
      <h3 class="section-title">{{ $t('analytics.builder.sections.projectsOverview') }}</h3>
      <div class="projects-grid">
        <ProjectCard
          v-for="project in dashboard.projectsOverview"
          :key="project.id"
          :project="project"
        />
      </div>
      <p v-if="!dashboard.projectsOverview?.length" class="no-data">{{ $t('analytics.builder.noData.projects') }}</p>
    </div>
  </div>
</template>

<style scoped>
.builder-dashboard {
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
  grid-template-columns: repeat(auto-fit, minmax(450px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.chart-container {
  background: white;
  border-radius: 0.75rem;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
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

.devices-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
}

.device-type-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #F9FAFB;
  border-radius: 0.5rem;
  transition: all 0.2s;
}

.device-type-card:hover {
  background: #F3F4F6;
  transform: translateY(-2px);
}

.device-type-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 0.5rem;
  background: white;
  color: #3B82F6;
  display: flex;
  align-items: center;
  justify-content: center;
}

.device-type-info {
  flex: 1;
}

.device-type-name {
  font-size: 0.875rem;
  font-weight: 600;
  color: #111827;
  margin-bottom: 0.125rem;
}

.device-type-count {
  font-size: 0.75rem;
  color: #6B7280;
}

.projects-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.25rem;
}

.no-data {
  text-align: center;
  color: #9CA3AF;
  padding: 2rem;
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .builder-dashboard {
    padding: 1rem;
  }
  
  .charts-grid {
    grid-template-columns: 1fr;
  }
  
  .projects-grid {
    grid-template-columns: 1fr;
  }
  
  .devices-grid {
    grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  }
}
</style>
