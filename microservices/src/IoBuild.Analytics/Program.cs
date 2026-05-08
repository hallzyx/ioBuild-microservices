using IoBuild.Analytics.Application.ACL;
using IoBuild.Analytics.Application.Internal.QueryServices;
using IoBuild.Analytics.Domain.Services;
using IoBuild.Analytics.Interfaces.ACL;
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

builder.Services.AddHttpClient<IDevicesContextFacade, DevicesContextFacade>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Services:DevicesApi") ?? "http://localhost:5003");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<IProjectsContextFacade, ProjectsContextFacade>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Services:ProjectsApi") ?? "http://localhost:5004");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();

builder.Services.AddScoped<AnalyticsDbContextInitializer>();

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
