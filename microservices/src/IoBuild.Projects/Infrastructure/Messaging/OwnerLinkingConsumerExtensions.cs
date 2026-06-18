namespace IoBuild.Projects.Infrastructure.Messaging;

/// <summary>
/// DI extension to register OwnerLinkingConsumer as a hosted background service
/// (PR 5, §3.1, REQ-OL-02).
/// </summary>
public static class OwnerLinkingConsumerExtensions
{
    /// <summary>
    /// Registers <see cref="OwnerLinkingConsumer"/> as a singleton <see cref="IHostedService"/>.
    /// Call this in <c>Program.cs</c> after <c>AddDomainEventPublishing</c>.
    /// </summary>
    public static IServiceCollection AddOwnerLinkingConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHostedService<OwnerLinkingConsumer>();
        return services;
    }
}
