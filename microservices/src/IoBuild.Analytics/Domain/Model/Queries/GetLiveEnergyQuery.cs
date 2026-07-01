namespace IoBuild.Analytics.Domain.Model.Queries;

public record GetBuilderLiveEnergyQuery(int UserId, int Minutes);
public record GetOwnerLiveEnergyQuery(int UserId, int Minutes);
