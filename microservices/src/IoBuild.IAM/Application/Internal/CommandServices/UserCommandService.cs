using IoBuild.IAM.Application.Internal.OutboundServices;
using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.IAM.Domain.Services;
using IoBuild.IAM.Interfaces.REST.Resources;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.IAM.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork) : IUserCommandService
{
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var user = await userRepository.FindByEmailAsync(command.Email);
        if (user is null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = tokenService.GenerateToken(user);
        return (user, token);
    }

    public async Task Handle(SignUpCommand command)
    {
        if (userRepository.ExistsByEmail(command.Email))
            throw new InvalidOperationException("A user with this email already exists.");

        var passwordHash = hashingService.HashPassword(command.Password);
        var user = new User(command.Email, passwordHash, command.Role);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
    }

    public async Task Handle(UpdatePasswordCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user is null)
            throw new KeyNotFoundException("User not found.");

        if (!hashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.UpdatePasswordHash(hashingService.HashPassword(command.NewPassword));
        userRepository.Update(user);
        await unitOfWork.CompleteAsync();
    }
}
