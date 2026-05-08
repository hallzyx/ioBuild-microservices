using IoBuild.IAM.Domain.Model.Aggregates;

namespace IoBuild.IAM.Application.Internal.OutboundServices;

public interface ITokenService
{
    string GenerateToken(User user);
    Task<int?> ValidateToken(string token);
}
