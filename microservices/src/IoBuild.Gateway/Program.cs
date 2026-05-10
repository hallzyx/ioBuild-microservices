using IoBuild.Shared.Infrastructure.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:8080");

// ── YARP Reverse Proxy ──
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Health Checks with downstream health probes ──
// Resolver URLs desde configuración YARP para compatibilidad con Docker
var clusters = builder.Configuration.GetSection("ReverseProxy:Clusters").GetChildren();
var healthChecks = builder.Services.AddHealthChecks();

foreach (var cluster in clusters)
{
    var clusterName = cluster.Key;
    var address = cluster.GetSection("Destinations").GetChildren()
        .FirstOrDefault()?.GetValue<string>("Address")?.TrimEnd('/');
    
    if (!string.IsNullOrEmpty(address))
    {
        var tags = clusterName switch
        {
            "iam-cluster" => new[] { "core" },
            "devices-cluster" => new[] { "core" },
            "projects-cluster" => new[] { "core" },
            "subscriptions-cluster" => new[] { "core" },
            _ => new[] { "optional" }
        };
        
        var healthUrl = $"{address}/health";
        var displayName = clusterName.Replace("-cluster", "").ToUpper();
        healthChecks.AddUrlGroup(new Uri(healthUrl), $"IoBuild.{displayName}", tags: tags);
    }
}

// ── HTTP Client Factory for downstream calls ──
builder.Services.AddHttpClient("InternalServices", client =>
{
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// ── Middleware Pipeline ──

// 1. Global Exception Handler (from IoBuild.Shared)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 2. CORS
app.UseCors("GatewayCorsPolicy");

// 3. Gateway Status
var gatewayInfo = new
{
    gateway = "IoBuild API Gateway",
    version = "1.0.0",
    status = "running",
    microservices = clusters.ToDictionary(
        c => c.Key.Replace("-cluster", ""),
        c => c.GetSection("Destinations").GetChildren()
            .FirstOrDefault()?.GetValue<string>("Address")?.TrimEnd('/') ?? "unknown"
    )
};

app.MapGet("/", () => Results.Ok(gatewayInfo));

// 4. Health Check Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            summary = $"{report.Entries.Count} services checked",
            services = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

// 5. YARP Reverse Proxy
app.MapReverseProxy();

app.Run();
