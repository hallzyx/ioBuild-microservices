using IoBuild.Analytics.Application.Internal.QueryServices;
using IoBuild.Analytics.Domain.Services;
using IoBuild.Analytics.Infrastructure.InfluxDB;
using IoBuild.Analytics.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using IoBuild.Analytics;
using IoBuild.Shared.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5005");

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "iobuild_analytics";
var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IoBuild.Analytics API", Version = "v1" });
    c.EnableAnnotations();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
});

// ACL HTTP facade registrations removed (IDevicesContextFacade / IProjectsContextFacade).
// Analytics now reads exclusively from local projection tables populated by
// AnalyticsEventConsumer (ADR-6, REQ-AQ-01).
// The facade classes are retained under Interfaces/ACL/ and Application/ACL/ for
// rollback reference — they are orphaned and no longer registered in DI.

// ── InfluxDB ──
var influxToken = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN") ?? "iobuild-telemetry-token";
builder.Services.Configure<InfluxDbOptions>(opts =>
{
    opts.Host = Environment.GetEnvironmentVariable("INFLUXDB_HOST") ?? "influxdb";
    opts.Port = 8086;
    opts.Token = influxToken;
    opts.Org = "iobuild";
    opts.Bucket = "iobuild-telemetry";
});
builder.Services.AddSingleton<ILiveEnergyService, LiveEnergyService>();

builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();

builder.Services.AddScoped<AnalyticsDbContextInitializer>();

// Register the RabbitMQ consumer that keeps the projection tables up to date (REQ-RM-02)
builder.Services.AddAnalyticsEventConsumer(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<AnalyticsDbContextInitializer>();
    await initializer.InitializeAsync();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IoBuild.Analytics API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
