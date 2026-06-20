/**
 * Subscription Entity
 * Represents a subscription with its plan details
 */
export class Subscription {
    constructor({
        id = 0,
        builderId = 0,
        plan = null,
        status = '',
        startDate = null,
        endDate = null
    }) {
        this.id = id;
        this.builderId = builderId;
        this.plan = plan; // This is now a Plan object
        this.status = status;
        this.startDate = startDate;
        this.endDate = endDate;
    }

    /**
     * Check if subscription is active
     */
    isActive() {
        // Backend serializes the status enum as PascalCase ("Active"); compare
        // case-insensitively so this doesn't silently always return false.
        return this.status?.toLowerCase() === 'active';
    }

    /**
     * Get formatted price from plan
     */
    getFormattedPrice() {
        return this.plan ? this.plan.getFormattedPrice() : '$0';
    }

    /**
     * Get plan name
     */
    getPlanName() {
        return this.plan ? this.plan.name : '';
    }
}
