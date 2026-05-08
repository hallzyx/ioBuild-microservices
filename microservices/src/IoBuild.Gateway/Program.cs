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
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("http://localhost:5001/health"), "IoBuild.IAM", tags: ["core"])
    .AddUrlGroup(new Uri("http://localhost:5002/health"), "IoBuild.Devices", tags: ["core"])
    .AddUrlGroup(new Uri("http://localhost:5003/health"), "IoBuild.Projects", tags: ["core"])
    .AddUrlGroup(new Uri("http://localhost:5004/health"), "IoBuild.Subscriptions", tags: ["core"])
    .AddUrlGroup(new Uri("http://localhost:5005/health"), "IoBuild.Analytics", tags: ["optional"]);

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
app.MapGet("/", () => Results.Ok(new
{
    gateway = "IoBuild API Gateway",
    version = "1.0.0",
    status = "running",
    microservices = new
    {
        iam = "http://localhost:5001",
        devices = "http://localhost:5002",
        projects = "http://localhost:5003",
        subscriptions = "http://localhost:5004",
        analytics = "http://localhost:5005"
    }
}));

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
