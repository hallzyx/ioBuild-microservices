using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.EntityFrameworkCore;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Infrastructure.InfluxDB;
using IoBuild.Devices.Infrastructure.Messaging;
using IoBuild.Devices.Infrastructure.Mqtt;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Devices.Workers;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using IoBuild.Shared.Infrastructure.Messaging;
using IoBuild.Shared.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5002");

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "iobuild_devices";
var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";

// ── JWT Secret: env var override > appsettings.json fallback ──
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration.GetValue<string>("TokenSettings:Secret")
    ?? "dev-fallback-key-minimum-32-characters!!";

builder.Services.Configure<TokenSettings>(options =>
{
    options.Secret = jwtSecret;
});

// ── InfluxDB Token: env var override > appsettings.json fallback ──
var influxDbToken = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN")
    ?? builder.Configuration.GetValue<string>("InfluxDb:Token")
    ?? "iobuild-telemetry-token-dev";

// ── InfluxDB ──
builder.Services.Configure<InfluxDbOptions>(builder.Configuration.GetSection(InfluxDbOptions.SectionName));
builder.Services.PostConfigure<InfluxDbOptions>(options =>
{
    options.Token = influxDbToken;
});
builder.Services.AddSingleton<ITelemetryWriteService, TelemetryWriteService>();
// ── MQTT ──
builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection(MqttOptions.SectionName));
// ── MQTT Publisher: singleton channel-backed publish service (ADR-B1) ──
// Registered as BOTH the hosted service AND the IMqttPublisher interface so handlers
// can inject the facade without a dependency on the concrete BackgroundService.
builder.Services.AddSingleton<MqttPublisherService>();
builder.Services.AddSingleton<IMqttPublisher>(sp => sp.GetRequiredService<MqttPublisherService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttPublisherService>());
// ── Telemetry Query ──
builder.Services.AddScoped<ITelemetryQueryService, TelemetryQueryService>();
// ── Telemetry Worker ──
builder.Services.AddHostedService<TelemetryWorker>();

builder.Services.AddHealthChecks();
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IoBuild.Devices", Version = "v1" });
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

builder.Services.AddDbContext<DevicesDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceLogRepository, DeviceLogRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();
builder.Services.AddScoped<IDeviceQueryService, DeviceQueryService>();
// ── Device Actuation: owner-gated command handler (T-2.13, ADR-B5) ──
builder.Services.AddScoped<IDeviceActuationService, DeviceActuationService>();
// ── Custom device type management (owner-custom-device-type feature) ──
builder.Services.AddScoped<CustomDeviceTypeCommandService>();
builder.Services.AddScoped<CustomDeviceTypeQueryService>();
// ── Device type catalog (device-type-catalog Slice 1, WU-1) ──
builder.Services.AddScoped<IDeviceTypeRepository, DeviceTypeRepository>();
builder.Services.AddScoped<DeviceTypeCatalogQueryService>();

// ── Domain-event publishing + outbox resilience pipeline (ADR-8) ──
builder.Services.AddDomainEventPublishing(builder.Configuration);

// ── OutboxWorker: polls pending outbox rows and publishes to RabbitMQ (ADR-2) ──
builder.Services.AddHostedService<OutboxWorker>();

// ── FloorProvisioningConsumer: seeds default devices when a floor is defined (PR 6, §4.1) ──
builder.Services.AddHostedService<FloorProvisioningConsumer>();

// ── UnitDeviceProvisioningConsumer: provisions selected devices per unit (T-17, ADR-2/D2) ──
builder.Services.AddHostedService<UnitDeviceProvisioningConsumer>();

// ── UnitOwnerProjectionConsumer: consumes UnitOwnerMatchedEvent → upserts projection (T-2.18, ADR-B4) ──
builder.Services.AddHostedService<UnitOwnerProjectionConsumer>();

// ── DeviceRegistryAnnouncer: re-announce all devices to MQTT registry/# on startup (simulator discovery) ──
builder.Services.AddHostedService<DeviceRegistryAnnouncer>();

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
    var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
    db.Database.Migrate();
    // Emit DeviceCreated events for seeded devices so the Analytics read model is populated
    // (HasData seed bypasses the command services / outbox). No-op once outbox has history.
    await OutboxBackfill.RunAsync(db);
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseJwtAuthentication();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
