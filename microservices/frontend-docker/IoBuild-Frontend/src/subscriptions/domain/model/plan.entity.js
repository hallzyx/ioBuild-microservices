/**
 * Plan Entity
 * Represents a subscription plan with its details and features
 */
export class Plan {
    constructor({
        id = 0,
        name = '',
        price = 0,
        description = '',
        features = [],
        maxDevices = 0,
        maxAdministrators = 0,
        supportLevel = '',
        hasAPI = false,
        hasAnalytics = false
    }) {
        this.id = id;
        this.name = name;
        this.price = price;
        this.description = description;
        this.features = features;
        this.maxDevices = maxDevices;
        this.maxAdministrators = maxAdministrators;
        this.supportLevel = supportLevel;
        this.hasAPI = hasAPI;
        this.hasAnalytics = hasAnalytics;
    }

    /**
     * Get formatted price per month
     */
    getFormattedPrice() {
        return `$${this.price}`;
    }

    /**
     * Check if it's an enterprise plan
     */
    isEnterprise() {
        return this.name.toLowerCase() === 'enterprise';
    }

    /**
     * Check if it's a professional plan
     */
    isProfessional() {
        return this.name.toLowerCase() === 'professional';
    }
}
