<script setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();

defineProps({
  plan: { type: Object, required: true },
  isProcessing: { type: Boolean, default: false }
});

defineEmits(['select']);
</script>

<template>
  <div
    class="border border-green-500 rounded-xl p-6 bg-white shadow-sm hover:shadow-md transition plan-card"
    :class="{ 'cursor-pointer': !isProcessing, 'cursor-not-allowed opacity-60': isProcessing }"
    @click="!isProcessing && $emit('select', plan)"
  >
    <h3 class="text-lg font-semibold text-gray-800 mb-1">
      {{ plan.name }}
    </h3>
    <p class="text-gray-600 mb-4">{{ plan.description }}</p>
    <p class="text-2xl font-bold text-gray-900 mb-4">
      {{ plan.getFormattedPrice() }}
      <span class="text-base font-normal text-gray-600">/{{ t("subscriptions.month") }}</span>
    </p>
    <ul class="space-y-2">
      <li
        v-for="(feature, i) in plan.features"
        :key="i"
        class="flex items-start text-gray-700"
      >
        <span class="text-green-500 mr-2">✔</span> {{ feature }}
      </li>
    </ul>
  </div>
</template>

<style scoped>
.custom-bg {
  background: linear-gradient(135deg, #f0fdf4 0%, #e6fffa 100%) !important;
}

li {
  line-height: 1.5rem;
}

/* Responsividad para las tarjetas de planes disponibles */
@media (max-width: 1024px) {
  .plan-card {
    padding: 1.25rem !important;
  }

  .plan-card h3 {
    font-size: 1rem !important;
  }

  .plan-card .text-2xl {
    font-size: 1.75rem !important;
  }

  .plan-card p {
    font-size: 0.875rem !important;
  }

  .plan-card li {
    font-size: 0.875rem !important;
  }
}

@media (max-width: 768px) {
  .plan-card {
    padding: 1rem !important;
    margin-bottom: 1rem;
  }

  .plan-card h3 {
    font-size: 1.125rem !important;
    text-align: center !important;
    margin-bottom: 0.75rem !important;
  }

  .plan-card .text-2xl {
    font-size: 1.5rem !important;
    text-align: center !important;
  }

  .plan-card .text-gray-600 {
    text-align: center !important;
  }

  .plan-card ul {
    margin-top: 1rem !important;
  }

  .plan-card li {
    font-size: 0.875rem !important;
    line-height: 1.4rem !important;
  }
}

@media (max-width: 480px) {
  .plan-card {
    padding: 0.875rem !important;
  }

  .plan-card h3 {
    font-size: 1rem !important;
  }

  .plan-card .text-2xl {
    font-size: 1.375rem !important;
  }

  .plan-card p {
    font-size: 0.8rem !important;
  }

  .plan-card li {
    font-size: 0.8rem !important;
  }

  .plan-card .text-xl {
    font-size: 1rem !important;
  }
}

/* Efectos hover solo en dispositivos que lo soportan */
@media (hover: hover) {
  .plan-card:hover {
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15) !important;
    transform: translateY(-2px);
    transition: all 0.3s ease;
  }
}

/* Para dispositivos táctiles, mejorar la accesibilidad */
@media (hover: none) {
  .plan-card {
    transition: none;
  }

  .plan-card:active {
    transform: scale(0.98);
    transition: transform 0.1s ease;
  }
}
</style>
