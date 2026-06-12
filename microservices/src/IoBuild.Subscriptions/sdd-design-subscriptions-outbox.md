# Design: Transactional Outbox + Webhooks Stripe para Suscripciones

## Technical Approach

Se implementa el patrón **Transactional Outbox** para garantizar consistencia eventual entre el estado de suscripciones en IoBuild.Subscriptions y los servicios downstream (IAM, Devices, Projects) que necesitan reaccionar a cambios de suscripción. Stripe webhooks reemplazan el polling manual como fuente de verdad para pagos completados, usando idempotency keys para tolerancia a duplicados. Un `BackgroundService` local procesa la outbox y notifica vía HTTP sin necesidad de broker externo.

Referencia de decisiones arquitectónicas: ADD v3 (CONTEXT.md), QA-3 (atomicidad transaccional).

## Architecture Decisions

| Decisión | Opciones | Tradeoff | Elegido |
|----------|----------|----------|---------|
| Idempotencia | Stripe Idempotency-Key nativa vs tabla propia | Stripe nativa requiere pasar key por header y no cubre renewals internos | **Tabla `idempotency_keys`** — cubre webhooks y renewals con同一 mecanismo |
| Broker | RabbitMQ vs HTTP directo desde OutboxWorker | RabbitMQ agrega latencia y运维, HTTP directo es más simple para 4 servicios | **HTTP directo** — el worker notifica vía HttpClient, sin broker externo |
| Outbox polling | EF Core query cada 5s vs SQL raw vs Change Tracking | EF query es simple pero agrega overhead; SQL raw es más eficiente pero rompe abstracción | **EF Core query** — prioriza consistencia de capas sobre micro-optimización |
| Webhook validation | Stripe-Events vs cuerpo plano | Stripe.Events requiere librería Stripe y verify del signature header | **Stripe.EventUtility.ConstructEvent** — ya tenemos Stripe.net instalado |
| Transacción ACID | UnitOfWork + transaction explícita vs SaveChanges sola | SaveChanges solo no protege contra fallo entre INSERT outbox y commit | **Transaction explícita** — `BeginTransactionAsync` + `CommitAsync` |

## Data Flow

### Flujo 1: Webhook Stripe (checkout.session.completed)

```
Stripe ──POST──→ WebhookController
                    │
                    ▼
              Stripe.EventUtility.ConstructEvent()
              (valida HMAC, extrae session)
                    │
                    ▼
              SubscriptionCommandService.HandleProcessCompletedCheckoutSession()
                    │
                    ├── 1. Check idempotency (IdempotencyKeyRepository.Exists)
                    │       └── si existe → return 200 OK (silent)
                    │
                    ├── 2. Iniciar transacción EF Core
                    │
                    ├── 3. subscription.Activate() + UpdateAsync()
                    ├── 4. OutboxMessage.Create("subscription.activated", payload)
                    ├── 5. IdempotencyKey.Create(eventId)
                    │
                    ├── 6. context.CompleteAsync()     ← SaveChanges (ACID)
                    └── 7. transaction.CommitAsync()
                         └── return 200 OK
```

### Flujo 2: Renewal manual (POST /renew)

```
Client ──POST──→ SubscriptionsController.RenewSubscription()
                    │
                    ▼
              SubscriptionCommandService.HandleRenewSubscription()
                    │
                    ├── 1. Check idempotency (key = "renew_{builderId}_{planId}")
                    │       └── si existe → 409 Conflict
                    │
                    ├── 2. subscription = FindByBuilderAsync(builderId)
                    ├── 3. StripePaymentService.CreateCheckoutSessionAsync()
                    │       └── Stripe Session (modo subscription)
                    │
                    ├── 4. Devolver session.Url al cliente
                    └── [el webhook de Stripe completa el flujo]
```

### Flujo 3: OutboxWorker procesa mensajes

```
OutboxWorker (cada 5s)
    │
    ├── 1. outboxRepo.GetPending() → lista de OutboxMessage con Status=Pending
    │
    └── por cada mensaje:
         │
         ├── 2. OutboxPublisher.Process(message)
         │       ├── POST http://iobuild-iam/api/v1/webhooks/subscription (si aplica)
         │       ├── POST http://iobuild-devices/api/v1/webhooks/subscription
         │       └── POST http://iobuild-projects/api/v1/webhooks/subscription
         │
         ├── 3. Si éxito → MarkProcessed(id)
         └── 4. Si falla → incrementa RetryCount
                 └── si RetryCount >= 3 → Status = Failed, guarda error
```

## File Changes

| File | Acción | Descripción |
|------|--------|-------------|
| `Domain/Model/Entities/OutboxMessage.cs` | **Create** | Entidad outbox con Id, EventType, Payload, Status, retry |
| `Domain/Model/Entities/IdempotencyKey.cs` | **Create** | Entidad idempotency con Key (PK string), CreatedAt, ExpiresAt |
| `Domain/Repositories/IOutboxMessageRepository.cs` | **Create** | Interface: GetPending, MarkProcessed, Create |
| `Domain/Repositories/IIdempotencyKeyRepository.cs` | **Create** | Interface: Exists, Create |
| `Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs` | **Create** | Implementación EF Core |
| `Infrastructure/Persistence/EFC/Repositories/IdempotencyKeyRepository.cs` | **Create** | Implementación EF Core |
| `Workers/OutboxWorker.cs` | **Create** | BackgroundService que poll cada 5s |
| `Workers/OutboxPublisher.cs` | **Create** | Procesa mensajes, HTTP a servicios, retry 3x |
| `Interfaces/REST/Controllers/WebhookController.cs` | **Create** | POST /webhooks/stripe, valida firma, delega a command service |
| `Domain/Model/Commands/ProcessCompletedCheckoutSession.cs` | **Create** | Command record para webhook |
| `Domain/Model/Commands/RenewSubscriptionCommand.cs` | **Create** | Command record para renewal |
| `Domain/Model/Queries/GetCurrentSubscriptionQuery.cs` | **Create** | Query record para current |
| `Domain/Repositories/Services/ISubscriptionCommandService.cs` | **Modify** | +Handle(ProcessCompletedCheckoutSession), +Handle(RenewSubscriptionCommand) |
| `Domain/Repositories/Services/ISubscriptionQueryService.cs` | **Modify** | +Handle(GetCurrentSubscriptionQuery) |
| `Application/Services/SubscriptionCommandService.cs` | **Modify** | Implementar RenewSubscription y ProcessCompletedCheckoutSession con ACID |
| `Application/Services/SubscriptionQueryService.cs` | **Modify** | Implementar GetCurrentSubscription |
| `Interfaces/REST/Controllers/SubscriptionsController.cs` | **Modify** | +current, +renew endpoints |
| `Infrastructure/Persistence/EFC/SubscriptionsDbContext.cs` | **Modify** | +DbSet<OutboxMessage>, +DbSet<IdempotencyKey>, +config OnModelCreating |
| `Infrastructure/Persistence/EFC/Repositories/SubscriptionRepository.cs` | **Modify** | +FindByBuilderAsync que retorna Subscription entity (no lista) |
| `Program.cs` | **Modify** | +DI para repos outbox/idempotency, +HostedService, +Stripe webhook secret |
| `Interfaces/REST/Resources/CurrentSubscriptionResource.cs` | **Create** | Resource record para response de current |
| `Interfaces/REST/Resources/RenewSubscriptionResource.cs` | **Create** | Resource record para request de renew |
| `Interfaces/REST/Assemblers/SubscriptionAssembler.cs` | **Modify** | +ToCurrentResource, +ToRenewCommand |

## Interfaces / Contracts

### Domain Entities

```csharp
// Domain/Model/Entities/OutboxMessage.cs
namespace IoBuild.Subscriptions.Domain.Model.Entities;

public class OutboxMessage
{
    public int Id { get; private set; }
    public string EventType { get; private set; }
    public string Payload { get; private set; }
    public string Status { get; private set; } = "Pending";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(string eventType, string payload)
    {
        EventType = eventType;
        Payload = payload;
    }

    public void MarkProcessed()
    {
        Status = "Processed";
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        RetryCount++;
        Error = error;
        if (RetryCount >= 3) Status = "Failed";
    }
}

// Domain/Model/Entities/IdempotencyKey.cs
namespace IoBuild.Subscriptions.Domain.Model.Entities;

public class IdempotencyKey
{
    public string Key { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }

    private IdempotencyKey() { }

    public IdempotencyKey(string key, TimeSpan ttl)
    {
        Key = key;
        ExpiresAt = DateTime.UtcNow.Add(ttl);
    }
}
```

### Domain Commands

```csharp
// Domain/Model/Commands/ProcessCompletedCheckoutSession.cs
namespace IoBuild.Subscriptions.Domain.Model.Commands;

public record ProcessCompletedCheckoutSession(
    string EventId,
    int BuilderId,
    int PlanId,
    string SessionId
);

// Domain/Model/Commands/RenewSubscriptionCommand.cs
namespace IoBuild.Subscriptions.Domain.Model.Commands;

public record RenewSubscriptionCommand(
    int BuilderId,
    int PlanId,
    string SuccessUrl,
    string CancelUrl
);
```

### Domain Queries

```csharp
// Domain/Model/Queries/GetCurrentSubscriptionQuery.cs
namespace IoBuild.Subscriptions.Domain.Model.Queries;

public record GetCurrentSubscriptionQuery(int BuilderId);
```

### Repository Interfaces

```csharp
// Domain/Repositories/IOutboxMessageRepository.cs
namespace IoBuild.Subscriptions.Domain.Repositories;

public interface IOutboxMessageRepository
{
    Task<IEnumerable<OutboxMessage>> GetPendingAsync();
    Task MarkProcessedAsync(int id);
    Task MarkFailedAsync(int id, string error);
    Task CreateAsync(OutboxMessage message);
}

// Domain/Repositories/IIdempotencyKeyRepository.cs
namespace IoBuild.Subscriptions.Domain.Repositories;

public interface IIdempotencyKeyRepository
{
    Task<bool> ExistsAsync(string key);
    Task CreateAsync(string key, TimeSpan? ttl = null);
}
```

### Service Interfaces (modificadas)

```csharp
// Domain/Repositories/Services/ISubscriptionCommandService.cs
public interface ISubscriptionCommandService
{
    Task<Subscription> Handle(CreateSubscriptionCommand command);
    Task<Subscription> Handle(UpdateSubscriptionCommand command);
    // NUEVOS:
    Task Handle(ProcessCompletedCheckoutSession command);  // webhook
    Task<string> Handle(RenewSubscriptionCommand command); // retorna checkout URL
}

// Domain/Repositories/Services/ISubscriptionQueryService.cs
public interface ISubscriptionQueryService
{
    Task<IEnumerable<Subscription>> Handle(GetAllSubscriptionsQuery query);
    Task<Subscription?> Handle(GetSubscriptionByIdQuery query);
    // NUEVO:
    Task<Subscription?> Handle(GetCurrentSubscriptionQuery query);
}
```

### Repository Implementations

```csharp
// Infrastructure/Persistence/EFC/Repositories/OutboxMessageRepository.cs
namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly SubscriptionsDbContext _context;

    public OutboxMessageRepository(SubscriptionsDbContext context) => _context = context;

    public async Task<IEnumerable<OutboxMessage>> GetPendingAsync() =>
        await _context.OutboxMessages
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync();

    public async Task MarkProcessedAsync(int id)
    {
        var msg = await _context.OutboxMessages.FindAsync(id);
        if (msg is not null) msg.MarkProcessed();
    }

    public async Task MarkFailedAsync(int id, string error)
    {
        var msg = await _context.OutboxMessages.FindAsync(id);
        if (msg is not null) msg.MarkFailed(error);
    }

    public async Task CreateAsync(OutboxMessage message) =>
        await _context.OutboxMessages.AddAsync(message);
}

// Infrastructure/Persistence/EFC/Repositories/IdempotencyKeyRepository.cs
namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

public class IdempotencyKeyRepository : IIdempotencyKeyRepository
{
    private readonly SubscriptionsDbContext _context;

    public IdempotencyKeyRepository(SubscriptionsDbContext context) => _context = context;

    public async Task<bool> ExistsAsync(string key) =>
        await _context.IdempotencyKeys.AnyAsync(k => k.Key == key);

    public async Task CreateAsync(string key, TimeSpan? ttl = null)
    {
        var entity = new IdempotencyKey(key, ttl ?? TimeSpan.FromHours(24));
        await _context.IdempotencyKeys.AddAsync(entity);
    }
}
```

### SubscriptionRepository — método adicional

```csharp
// Añadir a SubscriptionRepository.cs
public async Task<Subscription?> FindByBuilderAsync(int builderId)
{
    return await _context.Subscriptions
        .Include(s => s.Plan)
        .Where(s => s.BuilderId == builderId)
        .OrderByDescending(s => s.StartDate)
        .FirstOrDefaultAsync();
}
```

### CommandService — flujo ACID

```csharp
// Application/Services/SubscriptionCommandService.cs — nuevos métodos

public async Task Handle(ProcessCompletedCheckoutSession command)
{
    // 1. Idempotency check
    if (await _idempotencyRepo.ExistsAsync(command.EventId))
        return; // already processed

    var subscription = await _subscriptionRepo.FindByBuilderAsync(command.BuilderId)
        ?? throw new KeyNotFoundException($"No subscription found for builder {command.BuilderId}");

    // 2. ACID transaction
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        subscription.Activate();
        _subscriptionRepo.Update(subscription);

        var outbox = new OutboxMessage("subscription.activated",
            JsonSerializer.Serialize(new
            {
                BuilderId = command.BuilderId,
                PlanId = command.PlanId,
                Status = "Active",
                ActivatedAt = DateTime.UtcNow,
                SessionId = command.SessionId
            }));
        await _outboxRepo.CreateAsync(outbox);

        await _idempotencyRepo.CreateAsync(command.EventId);

        await _unitOfWork.CompleteAsync(); // SaveChangesAsync
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

public async Task<string> Handle(RenewSubscriptionCommand command)
{
    var idempotencyKey = $"renew_{command.BuilderId}_{command.PlanId}";
    if (await _idempotencyRepo.ExistsAsync(idempotencyKey))
        throw new InvalidOperationException("Renewal already in progress for this builder and plan.");

    // Crear la sesión de Stripe (no modifica estado aún)
    var (sessionId, checkoutUrl, _) = await _stripePaymentService
        .CreateCheckoutSessionAsync(command.BuilderId, command.PlanId,
            command.SuccessUrl, command.CancelUrl);

    // Registrar idempotency key para evitar duplicados
    await _idempotencyRepo.CreateAsync(idempotencyKey, TimeSpan.FromHours(1));

    return checkoutUrl;
}
```

### QueryService — nuevo método

```csharp
// Application/Services/SubscriptionQueryService.cs
public async Task<Subscription?> Handle(GetCurrentSubscriptionQuery query)
{
    var subscriptions = await _subscriptionRepository.FindByBuilderIdAsync(query.BuilderId);
    return subscriptions
        .OrderByDescending(s => s.StartDate)
        .FirstOrDefault(s => s.Status == SubscriptionStatus.Active
                          || s.Status == SubscriptionStatus.Pending);
}
```

### OutboxWorker

```csharp
// Workers/OutboxWorker.cs
namespace IoBuild.Subscriptions.Workers;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var outboxRepo = scope.ServiceProvider
                    .GetRequiredService<IOutboxMessageRepository>();
                var unitOfWork = scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();
                var publisher = scope.ServiceProvider
                    .GetRequiredService<OutboxPublisher>();

                var pending = await outboxRepo.GetPendingAsync();

                foreach (var msg in pending)
                {
                    try
                    {
                        await publisher.PublishAsync(msg, stoppingToken);
                        await outboxRepo.MarkProcessedAsync(msg.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process outbox message {Id}", msg.Id);
                        await outboxRepo.MarkFailedAsync(msg.Id, ex.Message);
                    }
                }

                if (pending.Any())
                    await unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxWorker iteration failed");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
```

### OutboxPublisher

```csharp
// Workers/OutboxPublisher.cs
namespace IoBuild.Subscriptions.Workers;

public class OutboxPublisher
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OutboxPublisher> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync(OutboxMessage message, CancellationToken ct)
    {
        var payload = new StringContent(message.Payload, Encoding.UTF8, "application/json");

        var targets = _configuration
            .GetSection("Outbox:Targets")
            .Get<List<string>>()
            ?? new List<string>();

        foreach (var target in targets)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"{target.TrimEnd('/')}/api/v1/webhooks/subscription",
                    payload, ct);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Delivered {EventType} to {Target}",
                    message.EventType, target);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to deliver {EventType} to {Target}: {Error}",
                    message.EventType, target, ex.Message);
                throw; // Will trigger retry
            }
        }
    }
}
```

### WebhookController

```csharp
// Interfaces/REST/Controllers/WebhookController.cs
namespace IoBuild.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly ISubscriptionCommandService _commandService;
    private readonly string _webhookSecret;

    public WebhookController(
        ISubscriptionCommandService commandService,
        IConfiguration configuration)
    {
        _commandService = commandService;
        _webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
            ?? configuration.GetValue<string>("Stripe:WebhookSecret")
            ?? throw new InvalidOperationException("STRIPE_WEBHOOK_SECRET not configured");
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _webhookSecret);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session is null) return BadRequest("Invalid session object");

                var builderId = int.Parse(session.Metadata["builder_id"]);
                var planId = int.Parse(session.Metadata["plan_id"]);

                await _commandService.Handle(new ProcessCompletedCheckoutSession(
                    EventId: stripeEvent.Id,
                    BuilderId: builderId,
                    PlanId: planId,
                    SessionId: session.Id
                ));
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

### SubscriptionsController — endpoints nuevos

```csharp
// Añadir a SubscriptionsController.cs

[HttpGet("current")]
public async Task<IActionResult> GetCurrentSubscription([FromQuery] int builderId)
{
    var query = new GetCurrentSubscriptionQuery(builderId);
    var subscription = await _queryService.Handle(query);

    if (subscription is null)
        return NotFound(new { message = "No active subscription found for this builder." });

    return Ok(SubscriptionAssembler.ToCurrentResource(subscription));
}

[HttpPost("renew")]
public async Task<IActionResult> RenewSubscription([FromBody] RenewSubscriptionResource resource)
{
    var command = SubscriptionAssembler.ToRenewCommand(resource);
    var checkoutUrl = await _commandService.Handle(command);
    return Ok(new { checkoutUrl });
}
```

### Resources nuevos

```csharp
// Interfaces/REST/Resources/CurrentSubscriptionResource.cs
public record CurrentSubscriptionResource(
    int Id,
    int BuilderId,
    int PlanId,
    string PlanName,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    PlanResource? Plan
);

// Interfaces/REST/Resources/RenewSubscriptionResource.cs
public record RenewSubscriptionResource(
    int BuilderId,
    int PlanId,
    string SuccessUrl,
    string CancelUrl
);
```

### DbContext — config adicional en OnModelCreating

```csharp
modelBuilder.Entity<OutboxMessage>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).ValueGeneratedOnAdd();
    entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
    entity.Property(e => e.Payload).IsRequired().HasColumnType("json");
    entity.Property(e => e.Error).HasMaxLength(1000);
    entity.HasIndex(e => new { e.Status, e.CreatedAt });
});

modelBuilder.Entity<IdempotencyKey>(entity =>
{
    entity.HasKey(e => e.Key);
    entity.Property(e => e.Key).HasMaxLength(255);
    entity.Property(e => e.ExpiresAt).IsRequired();
    entity.HasIndex(e => e.ExpiresAt);
});
```

### Program.cs — nuevas registraciones

```csharp
// ── Outbox ──
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
builder.Services.AddScoped<IIdempotencyKeyRepository, IdempotencyKeyRepository>();
builder.Services.AddHttpClient<OutboxPublisher>();
builder.Services.AddHostedService<OutboxWorker>();
```

## Flujo de Transacción ACID (QA-3)

La atomicidad se garantiza mediante una transacción explícita de base de datos que engloba 3 operaciones:

```
BeginTransactionAsync()
    ├── UPDATE subscription SET status = 'Active'       (escritura principal)
    ├── INSERT INTO outbox_messages (...)                 (evento para otros servicios)  
    └── INSERT INTO idempotency_keys (...)                (token de idempotencia)
CompleteAsync() → SaveChangesAsync() (commitea las 3 en EF)
CommitAsync()   → commitea en MySQL
```

**Si falla entre UPDATE y INSERT**: Rollback total — ni se activa la subscripción ni queda un outbox huérfano.
**Si falla después de INSERT pero antes de Commit**: Rollback — idempotency key no se persiste, el webhook se reintenta.
**Si el OutboxWorker falla al notificar**: El mensaje queda en Pending y se reintenta hasta 3 veces.
**Si el worker crashea antes de MarkProcessed**: Próximo ciclo pickea el mismo mensaje (al menos una vez).

```
                    ┌──────────────────────────────┐
                    │  Transacción MySQL            │
                    │  ┌──────┐ ┌──────┐ ┌──────┐  │
                    │  │ Subs │ │Outbox│ │Idemp │  │
                    │  │UPDATE│ │INSERT│ │INSERT│  │
                    │  └──────┘ └──────┘ └──────┘  │
                    │  SaveChangesAsync()            │
                    │  CommitAsync()                 │
                    └──────────────────────────────┘
                                │
                    Todo OK ────┴──── Rollback ──→ 200 OK (retry)
                                │
                      ┌─────────▼─────────┐
                      │  200 OK (Stripe)   │
                      └───────────────────┘
```

## Plan de Implementación (ordenado por dependencias)

| Paso | Descripción | Depende de |
|------|-------------|------------|
| **P1** | Crear `Domain/Model/Entities/OutboxMessage.cs` y `IdempotencyKey.cs` | — |
| **P2** | Crear `Domain/Repositories/IOutboxMessageRepository.cs` y `IIdempotencyKeyRepository.cs` | P1 |
| **P3** | Agregar config EF en `SubscriptionsDbContext` (DbSet, OnModelCreating) | P1 |
| **P4** | Crear `Infrastructure/.../Repositories/OutboxMessageRepository.cs` e `IdempotencyKeyRepository.cs` | P2, P3 |
| **P5** | Agregar `FindByBuilderAsync` a `SubscriptionRepository` | — |
| **P6** | Crear commands: `ProcessCompletedCheckoutSession`, `RenewSubscriptionCommand` | — |
| **P7** | Crear query: `GetCurrentSubscriptionQuery` | — |
| **P8** | Modificar `ISubscriptionCommandService` y `ISubscriptionQueryService` | P6, P7 |
| **P9** | Implementar `Handle(ProcessCompletedCheckoutSession)` ACID en `SubscriptionCommandService` | P4, P5, P8 |
| **P10** | Implementar `Handle(RenewSubscriptionCommand)` en `SubscriptionCommandService` | P4, P8 |
| **P11** | Implementar `Handle(GetCurrentSubscriptionQuery)` en `SubscriptionQueryService` | P8 |
| **P12** | Crear `Workers/OutboxWorker.cs` y `OutboxPublisher.cs` | P4 |
| **P13** | Crear `Interfaces/REST/Controllers/WebhookController.cs` | P9 |
| **P14** | Crear resources y assembler methods para current + renew | — |
| **P15** | Modificar `SubscriptionsController` (+current, +renew) | P10, P11, P14 |
| **P16** | Modificar `Program.cs` (DI, HostedService, webhook secret) | P4, P12, P13 |
| **P17** | Agregar sección `Outbox:Targets` a `appsettings.json` | — |

## Cobertura Specs vs Design

| Requisito / User Story | Archivo(s) que lo cubren |
|------------------------|--------------------------|
| **US28**: Ver plan y estado actual (`GET /api/v1/subscriptions/current?builderId={id}`) | `SubscriptionsController.cs` (+current), `SubscriptionQueryService.cs` (+GetCurrentSubscription), `GetCurrentSubscriptionQuery.cs`, `CurrentSubscriptionResource.cs` |
| **US31**: Renovar suscripción (`POST /api/v1/subscriptions/renew`) | `SubscriptionsController.cs` (+renew), `SubscriptionCommandService.cs` (+HandleRenew), `RenewSubscriptionCommand.cs`, `RenewSubscriptionResource.cs`, `IIdempotencyKeyRepository.cs` |
| **QA-3**: Consistencia eventual transaccional | `SubscriptionCommandService.cs` (HandleProcessCompletedCheckoutSession con transaction ACID), `OutboxMessage.cs`, `OutboxWorker.cs`, `OutboxPublisher.cs` |
| **QA-3**: Idempotencia en webhooks | `IdempotencyKey.cs`, `IIdempotencyKeyRepository.cs`, `IdempotencyKeyRepository.cs` |
| **Stripe webhook handler** (`POST /api/v1/webhooks/stripe`) | `WebhookController.cs`, `ProcessCompletedCheckoutSession.cs` |

## Testing Strategy

| Capa | Qué probar | Cómo |
|------|-----------|------|
| **Unit (Domain)** | OutboxMessage.MarkProcessed(), MarkFailed(), retry count | xUnit, test directo de entidad |
| **Unit (Domain)** | IdempotencyKey creation con TTL | xUnit |
| **Unit (Domain)** | Subscription.Activate() | Ya existe |
| **Integration (Infrastructure)** | OutboxMessageRepository.GetPendingAsync() | InMemory Database + EF Core |
| **Integration (Infrastructure)** | IdempotencyKeyRepository.ExistsAsync() | InMemory Database |
| **Integration (Application)** | Flujo ACID completo: subscription.Activate + outbox insert + idempotency insert en una transacción | InMemory Database + TransactionScope |
| **Integration (Workers)** | OutboxWorker poll + publish | Mock IOutboxMessageRepository + HttpClient |
| **Integration (API)** | WebhookController con Stripe.EventUtility | Mock HttpRequest con firmas válidas |
| **E2E** | Stripe webhook → activate subscription → outbox → notify | Postman collection o testcontainers |

## Open Questions

- [ ] ¿Cuáles son las URLs exactas de los servicios IAM, Devices y Projects para el OutboxPublisher? Se configuran en `appsettings.json` bajo `Outbox:Targets`.
- [ ] ¿Se necesita autenticación entre servicios para las notificaciones del outbox? (ej. API keys entre microservicios)
- [ ] ¿El TTL de idempotency keys (24h) es suficiente? Stripe puede reintentar webhooks hasta 3 días.
- [ ] ¿Seeder necesita datos de ejemplo de outbox/idempotency? No, son datos efímeros.

## Migration / Rollout

No se requiere migración de datos existentes. Las nuevas tablas `outbox_messages` e `idempotency_keys` se crean vía `EnsureCreated()` o migración EF. El flujo existente de `POST /payments/confirm` sigue funcionando en paralelo hasta que el webhook esté en producción.
