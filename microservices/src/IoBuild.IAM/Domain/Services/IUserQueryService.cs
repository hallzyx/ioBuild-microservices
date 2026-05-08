using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Domain.Services;

public interface IUserQueryService
{
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query);
    Task<User?> Handle(GetUserByIdQuery query);
    Task<User?> Handle(GetUserDetailByIdQuery query);
}
