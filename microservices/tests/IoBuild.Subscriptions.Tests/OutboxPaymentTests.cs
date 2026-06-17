using IoBuild.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Subscriptions.Domain.Model.Entities;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;
using IoBuild.Subscriptions.Application.Services;
using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Services;
using Moq;
using IoBuild.Shared.Domain.Repositories;
using StripeSettings = IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Configuration.StripeSettings;

namespace IoBuild.Subscriptions.Tests;

public class OutboxPaymentTests
{
    [Fact]
    public void OutboxMessage_Creation_SetsProperties()
    {
        // Arrange
        var eventType = "subscription.activated";
        var payload = "{\"builderId\":1,\"planId\":2}";

        // Act
        var message = new OutboxMessage(eventType, payload);

        // Assert
        Assert.Equal(eventType, message.EventType);
        Assert.Equal(payload, message.Payload);
        Assert.Equal("Pending", message.Status);
        Assert.Equal(0, message.RetryCount);
        Assert.Null(message.ProcessedAt);
        Assert.Null(message.Error);
    }

    [Fact]
    public void OutboxMessage_DefaultsAreCorrect()
    {
        // Arrange & Act
        var message = new OutboxMessage("test.event", "{}");

        // Assert
        Assert.Equal("Pending", message.Status);
        Assert.Equal(0, message.RetryCount);
        Assert.Null(message.ProcessedAt);
        Assert.Null(message.Error);
        Assert.True(message.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void IdempotencyKey_Creation_SetsProperties()
    {
        // Arrange
        var key = "renew_1_2";

        // Act
        var idempotencyKey = new IdempotencyKey(key);

        // Assert
        Assert.Equal(key, idempotencyKey.Key);
        Assert.True(idempotencyKey.CreatedAt <= DateTime.UtcNow);
        Assert.True(idempotencyKey.ExpiresAt > DateTime.UtcNow);
        Assert.True(idempotencyKey.ExpiresAt <= DateTime.UtcNow.AddHours(24));
    }

    [Fact]
    public async Task ProcessCompletedCheckoutSession_WhenNewSubscription_CreatesSubscriptionAndOutbox()
    {
        // Arrange
        var builderId = 1;
        var planId = 2;
        var eventId = "evt_test_123";
        var sessionId = "cs_test_abc";

        var subRepo = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();
        var idempotencyRepo = new Mock<IIdempotencyKeyRepository>();
        var planRepo = new Mock<IPlanRepository>();

        // Create a real StripeSettings with placeholder values for the mock
        var stripeSettings = new StripeSettings
        {
            SecretKey = "sk_test_placeholder",
            PublishableKey = "pk_test_placeholder",
            WebhookSecret = "whsec_placeholder"
        };
        var stripeService = new StripePaymentService(subRepo.Object, planRepo.Object, unitOfWork.Object, stripeSettings);

        // No existing active subscription
        subRepo.Setup(r => r.FindActiveByBuilderIdAsync(builderId))
            .ReturnsAsync((Subscription?)null);

        // No idempotency key yet
        idempotencyRepo.Setup(r => r.ExistsAsync(eventId))
            .ReturnsAsync(false);

        // Configure UnitOfWork
        unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        var service = new SubscriptionCommandService(
            subRepo.Object,
            unitOfWork.Object,
            outboxRepo.Object,
            idempotencyRepo.Object,
            planRepo.Object,
            stripeService);

        // Act
        var result = await service.ProcessCompletedCheckoutSessionAsync(eventId, sessionId, builderId, planId);

        // Assert
        Assert.True(result);

        // Should have created a subscription
        subRepo.Verify(r => r.AddAsync(It.Is<Subscription>(s =>
            s.BuilderId == builderId &&
            s.PlanId == planId &&
            s.Status == SubscriptionStatus.Active)), Times.Once);

        // Should have created an outbox message
        outboxRepo.Verify(r => r.AddAsync(It.Is<OutboxMessage>(m =>
            m.EventType == "subscription.activated" &&
            m.Status == "Pending")), Times.Once);

        // Should have recorded idempotency key
        idempotencyRepo.Verify(r => r.AddAsync(It.Is<IdempotencyKey>(k =>
            k.Key == eventId)), Times.Once);

        // Should have committed transaction
        unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessCompletedCheckoutSession_WhenIdempotencyKeyExists_ReturnsTrueWithoutSideEffects()
    {
        // Arrange
        var builderId = 1;
        var planId = 2;
        var eventId = "evt_test_456";
        var sessionId = "cs_test_def";

        var subRepo = new Mock<ISubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var outboxRepo = new Mock<IOutboxMessageRepository>();
        var idempotencyRepo = new Mock<IIdempotencyKeyRepository>();
        var planRepo = new Mock<IPlanRepository>();
        var stripeSettings = new StripeSettings
        {
            SecretKey = "sk_test_placeholder",
            PublishableKey = "pk_test_placeholder"
        };
        var stripeService = new StripePaymentService(subRepo.Object, planRepo.Object, unitOfWork.Object, stripeSettings);

        // Idempotency key already exists
        idempotencyRepo.Setup(r => r.ExistsAsync(eventId))
            .ReturnsAsync(true);

        var service = new SubscriptionCommandService(
            subRepo.Object,
            unitOfWork.Object,
            outboxRepo.Object,
            idempotencyRepo.Object,
            planRepo.Object,
            stripeService);

        // Act
        var result = await service.ProcessCompletedCheckoutSessionAsync(eventId, sessionId, builderId, planId);

        // Assert
        Assert.True(result);

        // No side effects - should not have created anything
        subRepo.Verify(r => r.AddAsync(It.IsAny<Subscription>()), Times.Never);
        outboxRepo.Verify(r => r.AddAsync(It.IsAny<OutboxMessage>()), Times.Never);
        unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }
}
