<script setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();

defineProps({
  plan: { type: Object, required: true },
  subscription: { type: Object, required: true },
  isProcessing: { type: Boolean, default: false }
});

defineEmits(['renew', 'cancel']);
</script>

<template>
  <div class="current-plan-card border border-green-500 rounded-xl p-5 shadow-lg bg-white">
    <h2 class="text-2xl font-bold mb-6 text-gray-800">
      {{ plan.description }}
    </h2>

    <div class="mb-6">
      <p class="text-4xl font-extrabold text-gray-900 mb-3">
        {{ plan.getFormattedPrice() }}
        <span class="text-lg font-normal text-gray-600">/{{ t("subscriptions.month") }}</span>
      </p>
      <p class="text-base text-gray-600" v-if="subscription.status">
        {{ t("subscriptions.status") }}:
        <span :class="subscription.isActive() ? 'text-green-600 font-bold text-lg' : 'text-red-600 font-bold text-lg'">
          {{ t(`subscriptions.status-${subscription.status}`) }}
        </span>
      </p>
    </div>

    <ul class="space-y-3 mb-8">
      <li
        v-for="(feature, i) in plan.features"
        :key="i"
        class="flex items-start text-gray-700 text-lg"
      >
        <span class="text-green-500 mr-3 text-xl">✔</span> {{ feature }}
      </li>
    </ul>

    <div class="flex flex-col gap-4">
      <pv-button
        :label="isProcessing ? 'Redirigiendo a Stripe...' : t('subscriptions.renew-plan')"
        class="custom-green-button-large"
        @click="$emit('renew')"
        :disabled="isProcessing"
        :loading="isProcessing"
      />
      <pv-button
        :label="t('subscriptions.cancel-plan')"
        outlined
        severity="danger"
        class="cancel-button-large"
        @click="$emit('cancel')"
        :disabled="!subscription.isActive() || isProcessing"
      />
    </div>
  </div>
</template>

<style scoped>
.current-plan-card {
  justify-self: baseline;
  max-width: 650px;
  transform: scale(1.05);
  border-width: 3px !important;
  box-shadow: 0 20px 50px rgba(16, 185, 129, 0.15), 0 10px 30px rgba(0, 0, 0, 0.1) !important;
  position: relative;
}

.current-plan-card::before {
  content: '';
  position: absolute;
  top: -2px;
  left: -2px;
  right: -2px;
  bottom: -2px;
  background: white;
  border-radius: 14px;
  z-index: -1;
  animation: glow 2s ease-in-out infinite alternate;
}

@keyframes glow {
  from { opacity: 0.6; }
  to { opacity: 0.9; }
}

/* Responsividad para la tarjeta del plan actual */
@media (max-width: 1024px) {
  .current-plan-card {
    transform: scale(1.02);
    max-width: 100%;
  }
}

@media (max-width: 768px) {
  .current-plan-card {
    transform: scale(1) !important;
    max-width: 100% !important;
    margin: 0 !important;
    padding: 1.5rem !important;
  }

  /* Ajustar tamaños de texto en móvil */
  .current-plan-card h2 {
    font-size: 1.5rem !important;
    margin-bottom: 1rem !important;
  }

  .current-plan-card .text-4xl {
    font-size: 2.5rem !important;
  }

  .current-plan-card .text-lg {
    font-size: 1rem !important;
  }

  .current-plan-card ul {
    margin-bottom: 1.5rem !important;
  }

  .current-plan-card li {
    font-size: 1rem !important;
  }
}

@media (max-width: 480px) {
  .current-plan-card {
    padding: 1rem !important;
  }

  .current-plan-card h2 {
    font-size: 1.25rem !important;
    text-align: center !important;
  }

  .current-plan-card .text-4xl {
    font-size: 2rem !important;
    text-align: center !important;
  }

  .current-plan-card .text-base {
    text-align: center !important;
  }
}

:deep(.custom-green-button-large) {
  background-color: #10B981 !important;
  border-color: #10B981 !important;
  color: white !important;
  padding: 14px 28px !important;
  font-size: 1.125rem !important;
  font-weight: 600 !important;
  border-radius: 8px !important;
}

:deep(.custom-green-button-large:hover) {
  background-color: #059669 !important;
  border-color: #059669 !important;
  transform: translateY(-2px) !important;
  box-shadow: 0 8px 25px rgba(16, 185, 129, 0.3) !important;
}

:deep(.custom-green-button-large:focus) {
  box-shadow: 0 0 0 0.2rem rgba(16, 185, 129, 0.5) !important;
}

:deep(.cancel-button-large) {
  padding: 14px 28px !important;
  font-size: 1.125rem !important;
  font-weight: 600 !important;
  border-radius: 8px !important;
}

:deep(.cancel-button-large:hover) {
  transform: translateY(-1px) !important;
}

/* Responsividad para botones */
@media (max-width: 768px) {
  :deep(.custom-green-button-large),
  :deep(.cancel-button-large) {
    width: 100% !important;
    padding: 12px 20px !important;
    font-size: 1rem !important;
  }
}

@media (max-width: 480px) {
  :deep(.custom-green-button-large),
  :deep(.cancel-button-large) {
    padding: 10px 16px !important;
    font-size: 0.9rem !important;
  }
}
</style>
