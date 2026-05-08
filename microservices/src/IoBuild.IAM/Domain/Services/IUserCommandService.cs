using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Interfaces.REST.Resources;

namespace IoBuild.IAM.Domain.Services;

public interface IUserCommandService
{
    Task<(User user, string token)> Handle(SignInCommand command);
    Task Handle(SignUpCommand command);
    Task Handle(UpdatePasswordCommand command);
}
