namespace IoBuild.Subscriptions.Domain.Model.Aggregates;

public class Plan
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Features { get; private set; } = string.Empty;
    public int MaxDevices { get; private set; }
    public int MaxAdministrators { get; private set; }
    public string SupportLevel { get; private set; } = string.Empty;
    public bool HasAPI { get; private set; }
    public bool HasAnalytics { get; private set; }
    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

    public Plan() { }

    public Plan(string name, decimal price, string description, string features,
        int maxDevices, int maxAdministrators, string supportLevel, bool hasApi, bool hasAnalytics)
    {
        Name = name;
        Price = price;
        Description = description;
        Features = features;
        MaxDevices = maxDevices;
        MaxAdministrators = maxAdministrators;
        SupportLevel = supportLevel;
        HasAPI = hasApi;
        HasAnalytics = hasAnalytics;
    }

    public void Update(string name, decimal price, string description, string features,
        int maxDevices, int maxAdministrators, string supportLevel, bool hasApi, bool hasAnalytics)
    {
        Name = name;
        Price = price;
        Description = description;
        Features = features;
        MaxDevices = maxDevices;
        MaxAdministrators = maxAdministrators;
        SupportLevel = supportLevel;
        HasAPI = hasApi;
        HasAnalytics = hasAnalytics;
    }
}
