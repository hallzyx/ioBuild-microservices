using System.Text.Json;
using IoBuild.IAM.Application.Internal.OutboundServices;
using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Domain.Model.Entities;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.IAM.Domain.Services;
using IoBuild.IAM.Interfaces.REST.Resources;
using IoBuild.Shared.Domain.Model.Events;
using IoBuild.Shared.Domain.Repositories;

namespace IoBuild.IAM.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork,
    IIamOutboxMessageRepository outboxRepository) : IUserCommandService
{
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var user = await userRepository.FindByEmailAsync(command.Email);
        if (user is null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = tokenService.GenerateToken(user);
        return (user, token);
    }

    /// <summary>
    /// Two-phase commit (ADR-A, §2.3):
    ///   Phase 1 — persist user → user.Id is now a real DB-assigned value.
    ///   Phase 2 — build UserRegisteredEvent with real UserId + lower-cased email,
    ///             persist outbox row, second CompleteAsync.
    /// A crash between the two commits leaves a user without an event; the
    /// OutboxBackfill re-emits on next startup (idempotent safety net).
    /// </summary>
    public async Task Handle(SignUpCommand command)
    {
        if (userRepository.ExistsByEmail(command.Email))
            throw new InvalidOperationException("A user with this email already exists.");

        var passwordHash = hashingService.HashPassword(command.Password);
        var user = new User(command.Email, passwordHash, command.Role);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync(); // (1) user.Id is now real

        var evt = new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = command.Email.ToLowerInvariant(), // ADR-D: always lower-case
            Role = command.Role
        };
        await outboxRepository.AddAsync(new OutboxMessage(nameof(UserRegisteredEvent),
            JsonSerializer.Serialize(evt)) { EventId = evt.EventId });
        await unitOfWork.CompleteAsync(); // (2) outbox row committed
    }
}
