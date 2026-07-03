import { BaseApi } from "../../shared/infrastructure/base-api.js";
import { BaseEndpoint } from "../../shared/infrastructure/base-endpoint.js";

const subscriptionsEndpointPath = import.meta.env.VITE_SUBSCRIPTIONS_ENDPOINT_PATH;

export class SubscriptionApi extends BaseApi {
    #subscriptionsEndpoint;

    constructor() {
        super();
        this.#subscriptionsEndpoint = new BaseEndpoint(this, subscriptionsEndpointPath);
    }

    async getSubscriptionByBuilderId(builderId) {
        const response = await this.#subscriptionsEndpoint.getAll();
        const allSubscriptions = response.data;
        // A builder can accumulate more than one subscription row over time
        // (e.g. cancel + renew); take the most recently started one.
        const mostRecent = allSubscriptions
            .filter(s => s.builderId === builderId)
            .sort((a, b) => new Date(b.startDate) - new Date(a.startDate))[0];
        return { data: mostRecent };
    }

    getSubscriptionById(id) {
        return this.#subscriptionsEndpoint.getById(id);
    }

    createSubscription(resource) {
        return this.#subscriptionsEndpoint.create(resource);
    }

    updateSubscription(resource) {
        return this.#subscriptionsEndpoint.update(resource.id, resource);
    }

    cancelSubscription(subscriptionId) {
        return this.http.post(`${subscriptionsEndpointPath}/${subscriptionId}/cancel`);
    }

    createCheckoutSession(builderId, planId) {
        // El backend espera las URLs de success y cancel
        // Stripe agrega automáticamente el session_id como query parameter
        const baseUrl = window.location.origin;
        const successUrl = `${baseUrl}/subscriptions/my-subscription?success=true`;
        const cancelUrl = `${baseUrl}/subscriptions/my-subscription?canceled=true`;

        return this.http.post(`${subscriptionsEndpointPath}/payments/sessions`, {
            builderId,
            planId,
            successUrl,
            cancelUrl
        });
    }

    confirmPayment(builderId, sessionId) {
        return this.http.patch(`${subscriptionsEndpointPath}/payments/sessions/${sessionId}`, {
            builderId,
            status: 'confirmed'
        });
    }

    getInvoicesByBuilder(builderId) {
        return this.http.get(`${subscriptionsEndpointPath}/payments/invoices`, { params: { builderId } });
    }
}
