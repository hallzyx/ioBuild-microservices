using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Infrastructure.Persistence;
using IoBuild.Projects.Domain.Repositories;
using IoBuild.Projects.Domain.Services;
using IoBuild.Projects.Infrastructure.Messaging;
using IoBuild.Projects.Infrastructure.Persistence;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Workers;
using IoBuild.Shared.Domain.Repositories;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using IoBuild.Shared.Infrastructure.Messaging;
using IoBuild.Shared.Infrastructure.Middleware;
using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5003");

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "iobuild_projects";
var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";

// ── JWT Secret: env var override > appsettings.json fallback ──
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration.GetValue<string>("TokenSettings:Secret")
    ?? "dev-fallback-key-minimum-32-characters!!";

builder.Services.Configure<TokenSettings>(options =>
{
    options.Secret = jwtSecret;
});

builder.Services.AddHealthChecks();
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IoBuild.Projects API",
        Version = "v1",
        Description = "Microservice for managing construction projects, units, and clients."
    });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

builder.Services.AddScoped<IProjectCommandService, ProjectCommandService>();
builder.Services.AddScoped<IProjectQueryService, ProjectQueryService>();
builder.Services.AddScoped<IUnitCommandService, UnitCommandService>();
builder.Services.AddScoped<IUnitQueryService, UnitQueryService>();
builder.Services.AddScoped<IClientCommandService, ClientCommandService>();
builder.Services.AddScoped<IClientQueryService, ClientQueryService>();

// PR 3 — Project structure definition command service (§1.3)
builder.Services.AddScoped<ProjectStructureCommandService>();

// ── Domain-event publishing + outbox resilience pipeline (ADR-8) ──
builder.Services.AddDomainEventPublishing(builder.Configuration);

// ── OutboxWorker: polls pending outbox rows and publishes to RabbitMQ (ADR-2) ──
builder.Services.AddHostedService<OutboxWorker>();

// ── UnitOwnerAnnouncer: re-publishes UnitOwnerMatchedEvent for all seeded units on startup ──
// Repopulates Devices.unit_owner_projections after a volume wipe (docker compose down -v).
// Publishes directly (not via outbox) because the consumer is idempotent (upsert by UnitId).
builder.Services.AddHostedService<UnitOwnerAnnouncer>();

// ── PR 5 — OwnerLinkingConsumer: subscribes to iam.user.# and backfills Unit.OwnerId (REQ-OL-02) ──
builder.Services.AddOwnerLinkingConsumer(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    // Emit Created events for seeded aggregates so the Analytics read model is populated
    // (HasData seed bypasses the command services / outbox). No-op once outbox has history.
    await OutboxBackfill.RunAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseJwtAuthentication();
app.UseCors();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
