namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record UpdatePlanResource(
    string Name,
    decimal Price,
    string Description,
    string Features,
    int MaxDevices,
    int MaxAdministrators,
    string SupportLevel,
    bool HasApi,
    bool HasAnalytics
);
