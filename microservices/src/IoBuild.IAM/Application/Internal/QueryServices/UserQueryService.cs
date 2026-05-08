using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.IAM.Domain.Services;
using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Application.Internal.QueryServices;

public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query)
    {
        return await userRepository.ListAsync();
    }

    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        return await userRepository.FindByIdAsync(query.UserId);
    }

    public async Task<User?> Handle(GetUserDetailByIdQuery query)
    {
        return await userRepository.FindByIdAsync(query.UserId);
    }
}
