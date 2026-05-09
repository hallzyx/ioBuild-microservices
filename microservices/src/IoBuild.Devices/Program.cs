using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.EntityFrameworkCore;
using IoBuild.Devices.Application.Internal.CommandServices;
using IoBuild.Devices.Application.Internal.QueryServices;
using IoBuild.Devices.Domain.Repositories;
using IoBuild.Devices.Domain.Services;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Devices.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
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
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();
builder.Services.AddScoped<IDeviceQueryService, DeviceQueryService>();

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
    db.Database.EnsureCreated();
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
