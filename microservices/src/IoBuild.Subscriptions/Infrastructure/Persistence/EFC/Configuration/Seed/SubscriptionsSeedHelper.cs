using IoBuild.Subscriptions.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Configuration.Seed;

/// <summary>
/// Runtime seed helper for Plans and Subscriptions.
/// Executed at startup after EnsureCreated() because Plan stores features as a serialized JSON string,
/// which is better handled at runtime than via HasData.
/// </summary>
public static class SubscriptionsSeedHelper
{
    public static void Seed(SubscriptionsDbContext context)
    {
        // Check if plans already exist
        if (context.Set<Plan>().Any())
        {
            return; // Data already seeded
        }

        // Seed Plans
        // Features are stored as JSON arrays to match the monolith's value converter format
        var starterPlan = new Plan(
            name: "Starter",
            price: 299m,
            description: "Perfect for small projects",
            features: System.Text.Json.JsonSerializer.Serialize(new List<string>
            {
                "Up to 50 IoT devices",
                "Basic dashboard",
                "Email support",
                "Updates included",
                "1 administrator",
                "Monthly reports"
            }),
            maxDevices: 50,
            maxAdministrators: 1,
            supportLevel: "Email",
            hasApi: false,
            hasAnalytics: false
        );

        var professionalPlan = new Plan(
            name: "Professional",
            price: 799m,
            description: "Ideal for medium-sized projects",
            features: System.Text.Json.JsonSerializer.Serialize(new List<string>
            {
                "Up to 200 IoT devices",
                "Advanced dashboard",
                "24/7 priority support",
                "Updates and new features",
                "3 administrators",
                "Real-time reports",
                "Custom API",
                "Training included"
            }),
            maxDevices: 200,
            maxAdministrators: 3,
            supportLevel: "24/7 priority",
            hasApi: true,
            hasAnalytics: true
        );

        var enterprisePlan = new Plan(
            name: "Enterprise",
            price: 1299m,
            description: "For big developments",
            features: System.Text.Json.JsonSerializer.Serialize(new List<string>
            {
                "Unlimited IoT devices",
                "Enterprise dashboard",
                "Dedicated 24/7 support",
                "Development of custom features",
                "Unlimited administrators",
                "Advanced analytics",
                "Complete API",
                "Specialized consulting",
                "Guaranteed SLA"
            }),
            maxDevices: -1, // Unlimited
            maxAdministrators: -1, // Unlimited
            supportLevel: "Dedicated 24/7",
            hasApi: true,
            hasAnalytics: true
        );

        context.Set<Plan>().AddRange(starterPlan, professionalPlan, enterprisePlan);
        context.SaveChanges();

        // Get the IDs of the saved plans
        var professionalPlanId = context.Set<Plan>().First(p => p.Name == "Professional").Id;
        var starterPlanId = context.Set<Plan>().First(p => p.Name == "Starter").Id;

        // Seed Subscriptions
        var subscription1 = new Subscription(
            builderId: 1,
            planId: professionalPlanId,
            startDate: new DateTime(2024, 1, 1),
            endDate: new DateTime(2025, 1, 1)
        );
        subscription1.Activate();

        var subscription2 = new Subscription(
            builderId: 2,
            planId: starterPlanId,
            startDate: new DateTime(2024, 6, 1),
            endDate: new DateTime(2025, 6, 1)
        );
        subscription2.Activate();

        context.Set<Subscription>().AddRange(subscription1, subscription2);
        context.SaveChanges();
    }
}
