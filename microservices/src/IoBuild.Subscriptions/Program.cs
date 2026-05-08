using IoBuild.Shared.Infrastructure.ASP.Configuration;
using IoBuild.Shared.Infrastructure.Middleware;
using IoBuild.Subscriptions.Application.Facades;
using IoBuild.Subscriptions.Application.Services;
using IoBuild.Subscriptions.Domain.Repositories;
using IoBuild.Subscriptions.Domain.Repositories.Services;
using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Configuration;
using IoBuild.Subscriptions.Infrastructure.Payment.Stripe.Services;
using IoBuild.Subscriptions.Infrastructure.Persistence.EFC;
using IoBuild.Subscriptions.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Port configuration
builder.WebHost.UseUrls("http://0.0.0.0:5004");

// Database
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "iobuild_subscriptions";
var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};";

builder.Services.AddDbContext<SubscriptionsDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// DbContext as IUnitOfWork
builder.Services.AddScoped<IoBuild.Shared.Domain.Repositories.IUnitOfWork>(sp =>
    sp.GetRequiredService<SubscriptionsDbContext>());

// Stripe: env var override > appsettings.json fallback
var stripeSettings = new StripeSettings
{
    SecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
        ?? builder.Configuration.GetValue<string>("Stripe:SecretKey")
        ?? "sk_test_placeholder",
    PublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY")
        ?? builder.Configuration.GetValue<string>("Stripe:PublishableKey")
        ?? "pk_test_placeholder"
};
builder.Services.AddSingleton(stripeSettings);
builder.Services.AddSingleton<StripePaymentService>();

// Repositories
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();

// Services
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();
builder.Services.AddScoped<IPlanCommandService, PlanCommandService>();
builder.Services.AddScoped<IPlanQueryService, PlanQueryService>();

// Facades
builder.Services.AddScoped<ISubscriptionsContextFacade, SubscriptionsContextFacade>();

// Controllers + kebab-case routes
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddHealthChecks();
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
    var db = scope.ServiceProvider.GetRequiredService<SubscriptionsDbContext>();
    db.Database.EnsureCreated();
}

// Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
