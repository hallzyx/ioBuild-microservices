<script setup>
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";

import useSubscriptionStore from "../../application/subscription.store.js";
import { IamFacade } from "../../infrastructure/iam.facade.js";
import PlanCard from "../components/plan-card.vue";
import CurrentPlanCard from "../components/current-plan-card.vue";
import { SubscriptionApi } from "../../infrastructure/subscription-api.js";

// Stripe
import { loadStripe } from "@stripe/stripe-js";

const { t } = useI18n();
const confirm = useConfirm();
const toast = useToast();
const store = useSubscriptionStore();
const subscriptionApi = new SubscriptionApi();

// Stripe setup - simplified like Profile.jsx
const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);
const isProcessing = ref(false);

// Helper function to get builderId from authenticated user using Facade (ACL)
const getBuilderId = () => {
  try {
    return IamFacade.getCurrentUserId();
  } catch (error) {
    console.error('[Subscriptions] Error getting user ID:', error);
    toast.add({
      severity: "error",
      summary: "Error de autenticación",
      detail: error.message,
      life: 5000
    });
    throw error;
  }
};

onMounted(async () => {
  store.fetchAvailablePlans();
  store.fetchCurrentSubscription();

  // Si volvemos del checkout de Stripe con success, confirmar y refrescar datos
  const urlParams = new URLSearchParams(window.location.search);
  const sessionId = urlParams.get('session_id');
  const hasSuccess = urlParams.get('success') === 'true';

  if (sessionId || hasSuccess) {
    try {
      // Confirmar el pago en el backend si tenemos sessionId
      if (sessionId) {
        const builderId = getBuilderId();
        await subscriptionApi.confirmPayment(builderId, sessionId);
      }

      // Mostrar mensaje de éxito
      toast.add({
        severity: "success",
        summary: t("subscriptions.success"),
        detail: t("subscriptions.payment-success") || "Pago realizado exitosamente",
        life: 5000
      });

      // Refrescar subscription
      await store.fetchCurrentSubscription();
    } catch (error) {
      console.error('Error confirming payment:', error);
      toast.add({
        severity: "warn",
        summary: t("subscriptions.warning") || "Advertencia",
        detail: "El pago fue procesado pero hubo un problema al actualizar. Recarga la página.",
        life: 5000
      });
    } finally {
      // Limpiar los parámetros de la URL sin recargar la página
      const cleanUrl = window.location.pathname;
      window.history.replaceState({}, document.title, cleanUrl);
    }
  }
});

const handleRenewPlan = async () => {
  await handlePayPlan(store.currentPlan);
};

const handleCancelPlan = () => {
  confirm.require({
    message: t("subscriptions.confirm-cancel"),
    header: t("subscriptions.cancel-header"),
    icon: "pi pi-exclamation-triangle",
    acceptClass: "p-button-danger",
    accept: async () => {
      try {
        await store.cancelSubscription();
        toast.add({
          severity: "success",
          summary: t("subscriptions.success"),
          detail: t("subscriptions.cancelled-successfully"),
          life: 3000,
        });
      } catch (error) {
        toast.add({
          severity: "error",
          summary: t("subscriptions.error"),
          detail: t("subscriptions.cancel-failed"),
          life: 3000,
        });
      }
    },
  });
};

const handleChangePlan = async (plan) => {
  await handlePayPlan(plan);
};

const handlePayPlan = async (plan) => {
  try {
    if (!import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY) {
      throw new Error('Falta configurar VITE_STRIPE_PUBLISHABLE_KEY en .env');
    }

    isProcessing.value = true;
    const builderId = getBuilderId();

    const { data } = await subscriptionApi.createCheckoutSession(builderId, plan.id);

    // El backend devuelve CheckoutUrl (con C mayúscula) según PaymentSessionResource
    if (data.checkoutUrl || data.CheckoutUrl) {
      window.location.href = data.checkoutUrl || data.CheckoutUrl;
      return;
    }

    // Si el backend devuelve un sessionId, usar redirectToCheckout
    if (data.sessionId || data.SessionId) {
      const stripe = await stripePromise;
      if (!stripe) {
        throw new Error('Stripe no se pudo inicializar');
      }

      const sessionId = data.sessionId || data.SessionId;
      const { error } = await stripe.redirectToCheckout({ sessionId });
      if (error) {
        console.error('Stripe redirect error:', error);
        toast.add({
          severity: "error",
          summary: t("subscriptions.error"),
          detail: 'No se pudo redirigir al pago. Inténtalo nuevamente.',
          life: 3000
        });
      }
      return;
    }

    throw new Error('No se recibió URL ni sessionId del servidor');
  } catch (err) {
    console.error('Payment error:', err);
    const message = err?.response?.data?.message || err?.message || 'No se pudo iniciar el pago';
    toast.add({
      severity: "error",
      summary: t("subscriptions.error"),
      detail: message,
      life: 3000
    });
  } finally {
    isProcessing.value = false;
  }
};


</script>

<template>
  <div>
    <div class="subscription-container">
      <div class="header-row">
        <div class="header-left">
          <h1 class="text-3xl font-extrabold text-gray-900 mb-1">
            {{ t("subscriptions.title") }}
          </h1>
          <p class="text-lg font-semibold text-gray-800">
            {{ t("subscriptions.current-plan") }}:
            <span class="text-black font-bold">
              {{ store.currentPlan?.name || "N/A" }}
            </span>
          </p>
        </div>
      </div>

      <div v-if="store.isLoading" class="flex justify-center items-center py-8">
        <pv-progress-spinner />
      </div>

      <div v-else-if="store.currentPlan" class="subscription-layout">
        <div class="current-plan-section">
          <CurrentPlanCard
            :plan="store.currentPlan"
            :subscription="store.currentSubscription"
            :isProcessing="isProcessing"
            @renew="handleRenewPlan"
            @cancel="handleCancelPlan"
          />
        </div>

        <div class="change-plan-section">
          <div
            class="change-plan-header bg-white border border-green-500 text-green-600 font-semibold px-6 py-4 rounded-xl mb-6 text-center shadow-sm"
          >
            {{ t("subscriptions.change-plan") }}
          </div>

          <div class="plan-cards-container">
            <PlanCard
              v-for="plan in store.otherPlans"
              :key="plan.name"
              :plan="plan"
              :isProcessing="isProcessing"
              @select="handleChangePlan"
            />
          </div>
        </div>
      </div>

      <div v-else class="text-center py-8">
        <p class="text-gray-500 mb-4">
          {{ t("subscriptions.no-subscription") }}
        </p>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <PlanCard
            v-for="plan in store.availablePlans"
            :key="plan.name"
            :plan="plan"
            :isProcessing="isProcessing"
            @select="handleChangePlan"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>

/* ==================== CONTENEDOR PRINCIPAL ==================== */
.subscription-container {
  padding: 2rem;
  min-height: 100vh;
  background: linear-gradient(135deg, #f0fdf4 0%, #e6fffa 100%);
  max-width: 1400px;
  margin: 0 auto;
}

li {
  line-height: 1.5rem;
}

.header-row {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}

.header-left p {
  margin-bottom: 0.5rem;
}

.header-row {
  justify-content: flex-start;
}

.subscription-layout {
  display: flex;
  flex-direction: row;
  gap: 2rem;
  align-items: flex-start;
  justify-content: flex-start;
  width: 100%;
  margin: 0 auto;
}

.current-plan-section {
  flex: 0 0 auto;
  width: 40%;
  max-width: 650px;
}

.change-plan-section {
  flex: 0 0 auto;
  width: 45%;
  max-width: 550px;
}

.change-plan-header {
  justify-self: center;
  border-radius: 10px;
}

.plan-cards-container {
  display: -webkit-box;
  -webkit-box-orient: horizontal;
  -webkit-box-direction: normal;
  -webkit-box-pack: justify;
  gap: 1rem;
  width: 100%;
}

/* ---------- MEDIA QUERIES ---------- */

@media (max-width: 1200px) {
  .subscription-container {
    padding: 1.5rem;
  }
  .subscription-layout {
    gap: 1.5rem;
    justify-content: space-between;
  }
  .change-plan-section {
    flex: 1;
    max-width: 45%;
    min-width: 280px;
  }
  .plan-cards-container {
    gap: 0.75rem;
  }
}

@media (max-width: 1024px) {
  .subscription-container {
    padding: 1.5rem;
  }
  .subscription-layout {
    gap: 1.5rem;
    justify-content: center;
  }
  .current-plan-section {
    flex: 1;
    max-width: 60%;
    min-width: 300px;
  }
  .change-plan-section {
    flex: 1;
    max-width: 40%;
    min-width: 250px;
  }
  .plan-cards-container {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    gap: 1rem;
  }
}

@media (max-width: 768px) {
  .subscription-container {
    padding: 1rem;
    text-align: center;
  }
  .header-row {
    flex-direction: column;
    align-items: center;
  }
  .subscription-layout {
    flex-direction: column;
    gap: 2rem;
    align-items: center;
  }
  .current-plan-section,
  .change-plan-section {
    flex: none;
    width: 100%;
    max-width: 500px;
  }
  .plan-cards-container {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-box-align: center;
    gap: 1.5rem;
  }
  .subscription-container h1 {
    font-size: 2rem;
  }
  .subscription-container p {
    font-size: 1rem;
  }
  .change-plan-header {
    font-size: 0.95rem;
    padding: 0.75rem 1.25rem;
  }
}

@media (max-width: 480px) {
  .subscription-container {
    padding: 0.75rem;
  }
  .subscription-layout {
    gap: 1.5rem;
  }
  .plan-cards-container {
    gap: 1rem;
  }
  .subscription-container h1 {
    font-size: 1.75rem;
    margin-bottom: 0.5rem;
  }
  .subscription-container p {
    font-size: 0.9rem;
    margin-bottom: 1.5rem;
  }
  .change-plan-header {
    font-size: 0.875rem;
    padding: 0.5rem 1rem;
    margin-bottom: 1rem;
  }
}

@media (max-width: 768px) {
  :deep(.current-plan-card) {
    transform: scale(1) !important;
    max-width: 100% !important;
    margin: 0 !important;
  }
}

@media (min-width: 1400px) {
  .subscription-container {
    max-width: 1400px;
    margin: 0 auto;
  }
  .subscription-layout {
    gap: 3rem;
  }
}
</style>
