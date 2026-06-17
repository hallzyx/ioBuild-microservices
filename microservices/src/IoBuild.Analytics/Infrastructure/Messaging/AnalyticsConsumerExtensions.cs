namespace IoBuild.Analytics.Infrastructure.Messaging;

/// <summary>
/// DI extension to register AnalyticsEventConsumer as a hosted background service
/// (ADR-8, REQ-RM-02, task 6.8).
/// </summary>
public static class AnalyticsConsumerExtensions
{
    public static IServiceCollection AddAnalyticsEventConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHostedService<AnalyticsEventConsumer>();
        return services;
    }
}
