namespace IoBuild.Subscriptions.Domain.Model.Commands;

public record CreatePlanCommand(
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
