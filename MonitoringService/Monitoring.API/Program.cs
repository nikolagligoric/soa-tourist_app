using Microsoft.EntityFrameworkCore;
using Monitoring.API.BackgroundServices;
using Monitoring.API.Data;
using Monitoring.API.Models;
using Monitoring.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<MonitoringDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    )
);

// Monitored services
var monitoredServices = builder.Configuration
    .GetSection("MonitoredServices")
    .Get<List<MonitoredServiceConfig>>()
    ?? new List<MonitoredServiceConfig>();


// HTTP clients for monitored services
foreach (var service in monitoredServices)
{
    var httpClientBuilder =
        builder.Services.AddHttpClient(
            service.Name,
            client =>
            {
                client.BaseAddress =
                    new Uri(service.BaseUrl);
            }
        );

    // Stakeholders and Blog use development HTTPS
    if (service.BaseUrl.StartsWith(
            "https://",
            StringComparison.OrdinalIgnoreCase))
    {
        httpClientBuilder
            .ConfigurePrimaryHttpMessageHandler(
                () => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler
                            .DangerousAcceptAnyServerCertificateValidator
                }
            );
    }
}


// Monitored services through DI
builder.Services.AddSingleton(
    monitoredServices
);

// Current status store
builder.Services.AddSingleton<ServiceStatusStore>();


// Background monitoring
builder.Services.AddHostedService<
    HealthMonitoringBackgroundService>();

builder.Services.AddHostedService<
    LogMonitoringBackgroundService>();


// Loki
var lokiBaseUrl =
    builder.Configuration["Loki:BaseUrl"]
    ?? "http://localhost:3100";

builder.Services.AddHttpClient(
    "Loki",
    client =>
    {
        client.BaseAddress =
            new Uri(lokiBaseUrl);
    }
);


// OpenAPI
builder.Services.AddOpenApi();


var app = builder.Build();


// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<MonitoringDbContext>();

    dbContext.Database.Migrate();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();