using FluentAssertions;
using IoBuild.IAM.Application.Internal.CommandServices;
using IoBuild.IAM.Application.Internal.OutboundServices;
using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Infrastructure.Hashing.BCrypt.Services;
using IoBuild.IAM.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.IAM.Infrastructure.Tokens.JWT.Services;
using IoBuild.IAM.Interfaces.REST.Resources;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IoBuild.IAM.Tests.Application;

/// <summary>
/// RED tests for task 4.1 — IAM SignUp outbox behavior (ADR-A two-phase commit).
/// Verifies that after Handle(SignUpCommand), one outbox row is written with:
/// - EventType = "UserRegisteredEvent"
/// - Payload UserId > 0 (real Id after first CompleteAsync)
/// - Payload Email lower-cased
/// </summary>
public class IamSignUpOutboxTests
{
    private static ApplicationDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static UserCommandService BuildService(ApplicationDbContext ctx)
    {
        var baseRepo = new BaseRepository<User>(ctx);
        var userRepo = new UserRepository(baseRepo);
        var outboxRepo = new IoBuild.IAM.Infrastructure.Persistence.EFC.Repositories.OutboxMessageRepository(ctx);
        var hashingService = new HashingService();
        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("mock-token");
        return new UserCommandService(userRepo, tokenServiceMock.Object, hashingService, ctx, outboxRepo);
    }

    [Fact]
    public async Task Handle_SignUp_WritesOutboxRow_WithUserRegisteredEvent()
    {
        // Arrange
        var dbName = $"iam-signup-outbox-{Guid.NewGuid()}";
        await using var ctx = BuildContext(dbName);
        var service = BuildService(ctx);

        // Act
        await service.Handle(new SignUpCommand("OWNER@Test.COM", "Password123!", "owner"));

        // Assert — one outbox row
        var messages = ctx.OutboxMessages.ToList();
        messages.Should().HaveCount(1);

        var msg = messages[0];
        msg.EventType.Should().Be("UserRegisteredEvent");
        msg.Status.Should().Be("Pending");

        // Payload should have UserId > 0 and lower-cased email
        var payload = System.Text.Json.JsonDocument.Parse(msg.Payload);
        payload.RootElement.GetProperty("UserId").GetInt32().Should().BeGreaterThan(0);
        payload.RootElement.GetProperty("Email").GetString().Should().Be("owner@test.com");
        payload.RootElement.GetProperty("Role").GetString().Should().Be("owner");
    }

    [Fact]
    public async Task Handle_SignUp_EmailLowerCased_InOutboxPayload()
    {
        // Arrange — email with mixed case
        var dbName = $"iam-signup-lowercase-{Guid.NewGuid()}";
        await using var ctx = BuildContext(dbName);
        var service = BuildService(ctx);

        // Act
        await service.Handle(new SignUpCommand("BUILDER@iobuilt.COM", "Password123!", "builder"));

        // Assert
        var msg = ctx.OutboxMessages.Single();
        var payload = System.Text.Json.JsonDocument.Parse(msg.Payload);
        payload.RootElement.GetProperty("Email").GetString().Should().Be("builder@iobuilt.com");
    }

    [Fact]
    public async Task Handle_SignUp_UserId_IsPositive_AfterFirstCompleteAsync()
    {
        // Arrange — verifies two-phase commit gives real UserId (EF InMemory assigns sequential ids)
        var dbName = $"iam-signup-userid-{Guid.NewGuid()}";
        await using var ctx = BuildContext(dbName);
        var service = BuildService(ctx);

        // Act
        await service.Handle(new SignUpCommand("newuser@test.com", "Password123!", "owner"));

        // Assert
        var msg = ctx.OutboxMessages.Single();
        var payload = System.Text.Json.JsonDocument.Parse(msg.Payload);
        payload.RootElement.GetProperty("UserId").GetInt32().Should().BeGreaterThan(0,
            "two-phase commit ensures UserId is set after first CompleteAsync (ADR-A)");
    }
}
