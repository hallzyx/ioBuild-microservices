import { Subscription } from "../domain/model/subscription.entity.js";
import { PlanAssembler } from "./plan.assembler.js";

/**
 * Subscription Assembler
 * Handles the conversion between API resources and Subscription entities
 */
export class SubscriptionAssembler {
    /**
     * Convert API resource to Subscription entity
     */
    static toEntityFromResource(resource) {
        return new Subscription({
            id: resource.id,
            builderId: resource.builderId,
            plan: resource.plan ? PlanAssembler.toEntityFromResource(resource.plan) : null,
            status: resource.status,
            startDate: resource.startDate,
            endDate: resource.endDate
        });
    }

    /**
     * Convert API response to Subscription entity
     */
    static toEntityFromResponse(response) {
        if (!response.data) return null;
        return this.toEntityFromResource(response.data);
    }

    /**
     * Convert Subscription entity to API resource
     */
    static toResourceFromEntity(entity) {
        return {
            id: entity.id,
            builderId: entity.builderId,
            planId: entity.plan ? entity.plan.id : null,
            status: entity.status,
            startDate: entity.startDate,
            endDate: entity.endDate
        };
    }
}
