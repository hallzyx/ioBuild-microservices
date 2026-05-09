using IoBuild.IAM.Application.Internal.CommandServices;
using IoBuild.IAM.Application.Internal.OutboundServices;
using IoBuild.IAM.Application.Internal.QueryServices;
using IoBuild.IAM.Domain.Repositories;
using IoBuild.IAM.Domain.Services;
using IoBuild.IAM.Infrastructure.Hashing.BCrypt.Services;
using IoBuild.IAM.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using IoBuild.IAM.Infrastructure.Tokens.JWT.Services;
using IoBuild.Shared.Domain.Repositories;
using IoBuild.Shared.Infrastructure.ASP.Configuration;
using IoBuild.Shared.Infrastructure.Middleware;
using IoBuild.Shared.Infrastructure.Tokens;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5001");

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "iobuild_iam";

var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();

// ── JWT Revocation (QA-1) ──
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

// ── JWT Secret: env var override > appsettings.json fallback ──
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration.GetValue<string>("TokenSettings:Secret")
    ?? "dev-fallback-key-minimum-32-characters!!";

builder.Services.Configure<TokenSettings>(options =>
{
    options.Secret = jwtSecret;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IoBuild.IAM", Version = "v1" });
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

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
});

var app = builder.Build();

// ── Database Auto-Creation (Dev) ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseRequestAuthorization();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
