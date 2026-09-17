using Microsoft.EntityFrameworkCore;
using Monitoring.API.BackgroundServices;
using Monitoring.API.Data;
using Monitoring.API.Models;
using Monitoring.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services.AddDbContext<MonitoringDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// Ucitavanje servisa iz appsettings.json
var monitoredServices = builder.Configuration
    .GetSection("MonitoredServices")
    .Get<List<MonitoredServiceConfig>>()
    ?? new List<MonitoredServiceConfig>();

// HttpClient za svaki servis koji pratimo
foreach (var service in monitoredServices)
{
    builder.Services.AddHttpClient(service.Name, client =>
    {
        client.BaseAddress = new Uri(service.BaseUrl);
    });
}

// Lista servisa dostupna kroz Dependency Injection
builder.Services.AddSingleton(monitoredServices);

// Store za trenutne statuse
builder.Services.AddSingleton<ServiceStatusStore>();

// Background health monitoring
builder.Services.AddHostedService<HealthMonitoringBackgroundService>();
builder.Services.AddHostedService<LogMonitoringBackgroundService>();

builder.Services.AddOpenApi();

builder.Services.AddHttpClient("Loki", client =>
{
    client.BaseAddress = new Uri("http://localhost:3100");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();