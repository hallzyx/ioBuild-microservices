import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { SubscriptionApi } from "../infrastructure/subscription-api.js";
import { SubscriptionAssembler } from "../infrastructure/subscription.assembler.js";
import { PlanApi } from "../infrastructure/plan-api.js";
import { PlanAssembler } from "../infrastructure/plan.assembler.js";
import { IamFacade } from "../infrastructure/iam.facade.js";

const subscriptionApi = new SubscriptionApi();
const planApi = new PlanApi();

export const useSubscriptionStore = defineStore("subscriptions", () => {
    const currentSubscription = ref(null);
    const availablePlans = ref([]);
    const errors = ref([]);
    const isLoading = ref(false);

    // Helper function to get builderId from authenticated user using Facade (ACL)
    const getBuilderId = () => {
        try {
            return IamFacade.getCurrentUserId();
        } catch (error) {
            console.error('[SubscriptionStore] Error getting user ID:', error);
            throw error;
        }
    };

    // Fetch available plans from API
    function fetchAvailablePlans() {
        isLoading.value = true;
        return planApi
            .getAllPlans()
            .then((response) => {
                if (response.data) {
                    availablePlans.value = PlanAssembler.toEntitiesFromResourceArray(response.data);
                }
                isLoading.value = false;
            })
            .catch((error) => {
                errors.value.push(error);
                isLoading.value = false;
            });
    }

    const currentPlan = computed(() => {
        if (!currentSubscription.value || !currentSubscription.value.plan) return null;
        return currentSubscription.value.plan;
    });

    const otherPlans = computed(() => {
        if (!currentSubscription.value || !currentSubscription.value.plan) return availablePlans.value;
        return availablePlans.value.filter(plan =>
            plan.id !== currentSubscription.value.plan.id
        );
    });

    function fetchCurrentSubscription() {
        isLoading.value = true;
        const builderId = getBuilderId();
        return subscriptionApi
            .getSubscriptionByBuilderId(builderId)
            .then((response) => {
                currentSubscription.value = response.data
                    ? SubscriptionAssembler.toEntityFromResponse(response)
                    : null;
                isLoading.value = false;
            })
            .catch((error) => {
                // 404 means this builder has no subscription — clear any stale cached data.
                if (error?.response?.status === 404) {
                    currentSubscription.value = null;
                } else {
                    errors.value.push(error);
                }
                isLoading.value = false;
            });
    }

    function cancelSubscription() {
        if (!currentSubscription.value) return Promise.reject('No current subscription');

        isLoading.value = true;
        return subscriptionApi
            .cancelSubscription(currentSubscription.value.id)
            .then(() => {
                // Refresh current subscription
                return fetchCurrentSubscription();
            })
            .catch((error) => {
                errors.value.push(error);
                isLoading.value = false;
                throw error;
            });
    }

    function clearUserSession() {
        currentSubscription.value = null;
        errors.value = [];
    }

    return {
        currentSubscription,
        availablePlans,
        currentPlan,
        otherPlans,
        errors,
        isLoading,
        fetchCurrentSubscription,
        fetchAvailablePlans,
        cancelSubscription,
        clearUserSession
    };
});

export default useSubscriptionStore;

