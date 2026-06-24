using IoBuild.Subscriptions.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Configuration.Seed;

/// <summary>
/// Runtime seed helper for subscription Plans.
/// Executed at startup after EnsureCreated() because Plan stores features as a serialized
/// JSON string, which is better handled at runtime than via HasData.
///
/// WU-3 seed reset: the two demo Subscription rows (builderId=1, builderId=2) have been
/// removed. Only Plan rows are seeded. A fresh stack starts with 0 rows in `subscriptions`.
///
/// IMPORTANT — EnsureCreated stale-volume trap:
/// EnsureCreated never re-runs once the schema exists, so removed rows here WILL survive
/// on an existing Docker volume. To fully reset to demo-from-zero, the operator MUST run:
///   docker compose down -v
/// before the first start of this slice. Established convention — see memory:
/// "Analytics EnsureCreated schema trap".
/// </summary>
public static class SubscriptionsSeedHelper
{
    public static void Seed(SubscriptionsDbContext context)
    {
        // Idempotency guard: if plans already exist, the full seed ran — skip.
        if (context.Set<Plan>().Any())
        {
            return;
        }

        // ==================== SEED PLANS ====================
        // Features are stored as JSON arrays to match the monolith's value converter format.
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

        // ==================== DEMO SUBSCRIPTIONS REMOVED ====================
        // WU-3 seed reset: subscription1 (builderId=1, Professional) and
        // subscription2 (builderId=2, Starter) have been removed.
        // A fresh stack starts with 0 rows in `subscriptions`.
        // Builders subscribe through the normal subscription flow.
    }
}
