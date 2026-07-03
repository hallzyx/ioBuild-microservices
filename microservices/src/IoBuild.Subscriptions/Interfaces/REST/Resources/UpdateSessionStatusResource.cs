namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record UpdateSessionStatusResource(
    int BuilderId,
    string Status
);
