using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Monitoring.API.Data;
using Monitoring.API.Models;
using Monitoring.API.Services;

namespace Monitoring.API.BackgroundServices;

public class HealthMonitoringBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HealthMonitoringBackgroundService> _logger;
    private readonly ServiceStatusStore _statusStore;
    private readonly List<MonitoredServiceConfig> _monitoredServices;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly Dictionary<string, string> _previousStatuses = new();

    public HealthMonitoringBackgroundService(
        IHttpClientFactory httpClientFactory,
        ILogger<HealthMonitoringBackgroundService> logger,
        ServiceStatusStore statusStore,
        List<MonitoredServiceConfig> monitoredServices,
        IServiceScopeFactory scopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _statusStore = statusStore;
        _monitoredServices = monitoredServices;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var service in _monitoredServices)
            {
                var activeInstanceCount = 0;
                var responseTimes = new List<double>();


                var instanceHealthChecks = new List<InstanceHealthCheckHistory>();
                /*
                 * INSTANCE HEALTH CHECKS
                 */

                foreach (var instance in service.Instances)
                {
                    var client =
                        _httpClientFactory.CreateClient(service.Name);

                    string instanceStatus;
                    double? instanceResponseTimeMs = null;

                    var stopwatch = Stopwatch.StartNew();

                    try
                    {
                        var healthUrl =
                            $"{instance.BaseUrl.TrimEnd('/')}" +
                            $"{instance.HealthEndpoint}";

                        var response = await client.GetAsync(
                            healthUrl,
                            stoppingToken);

                        stopwatch.Stop();

                        instanceResponseTimeMs =
                            stopwatch.Elapsed.TotalMilliseconds;

                        instanceStatus = response.IsSuccessStatusCode
                            ? "UP"
                            : "DOWN";
                    }
                    catch (Exception)
                    {
                        stopwatch.Stop();

                        instanceStatus = "DOWN";
                    }

                    var checkedAt = DateTime.UtcNow;

                    var previousInstanceStatus =
                        _statusStore.GetInstanceStatus(
                            service.Name,
                            instance.InstanceId);

                    DateTime? lastSuccessfulCheckAt;

                    if (instanceStatus == "UP")
                    {
                        lastSuccessfulCheckAt = checkedAt;
                        activeInstanceCount++;

                        if (instanceResponseTimeMs.HasValue)
                        {
                            responseTimes.Add(
                                instanceResponseTimeMs.Value);
                        }
                    }
                    else
                    {
                        lastSuccessfulCheckAt =
                            previousInstanceStatus?
                                .LastSuccessfulCheckAt;
                    }

                    _statusStore.SetInstanceStatus(
                        new InstanceHealthStatus
                        {
                            ServiceName = service.Name,
                            InstanceId = instance.InstanceId,
                            Status = instanceStatus,
                            LastCheckedAt = checkedAt,
                            LastSuccessfulCheckAt =
                                lastSuccessfulCheckAt,
                            ResponseTimeMs =
                                instanceResponseTimeMs
                        });

                    instanceHealthChecks.Add(
                        new InstanceHealthCheckHistory
                        {
                            ServiceName = service.Name,
                            InstanceId = instance.InstanceId,
                            Status = instanceStatus,
                            ResponseTimeMs = instanceResponseTimeMs,
                            CheckedAt = checkedAt
                        });

                    if (previousInstanceStatus?.Status
                        != instanceStatus)
                    {
                        _logger.LogInformation(
                            "{ServiceName}/{InstanceId} status changed: {PreviousStatus} -> {CurrentStatus}",
                            service.Name,
                            instance.InstanceId,
                            previousInstanceStatus?.Status
                                ?? "UNKNOWN",
                            instanceStatus);
                    }
                }

                /*
                 * SERVICE STATUS
                 *
                 * Servis je UP ako ima bar jednu aktivnu instancu.
                 * DOWN je samo ako nijedna instanca nije dostupna.
                 */

                var currentStatus =
                    activeInstanceCount > 0
                        ? "UP"
                        : "DOWN";

                double? responseTimeMs =
                    responseTimes.Count > 0
                        ? responseTimes.Average()
                        : null;

                // Cuvanje trenutnog stanja servisa u memoriji
                var serviceCheckedAt = DateTime.UtcNow;

                var previousServiceStatus =
                    _statusStore.GetStatus(service.Name);

                DateTime? serviceLastSuccessfulCheckAt;

                if (currentStatus == "UP")
                {
                    serviceLastSuccessfulCheckAt = serviceCheckedAt;
                }
                else
                {
                    serviceLastSuccessfulCheckAt =
                        previousServiceStatus?.LastSuccessfulCheckAt;
                }

                _statusStore.SetStatus(
                    new ServiceHealthStatus
                    {
                        ServiceName = service.Name,
                        Status = currentStatus,
                        LastCheckedAt = serviceCheckedAt,
                        LastSuccessfulCheckAt =
                            serviceLastSuccessfulCheckAt,
                        ResponseTimeMs = responseTimeMs
                    });

                using var scope =
                    _scopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<MonitoringDbContext>();

                /*
                 * CUVANJE SERVICE HEALTH ISTORIJE
                 */

                dbContext.HealthCheckHistories.Add(
                    new HealthCheckHistory
                    {
                        ServiceName = service.Name,
                        Status = currentStatus,
                        ResponseTimeMs = responseTimeMs,
                        CheckedAt = DateTime.UtcNow
                    });

                dbContext.InstanceHealthCheckHistories
                    .AddRange(instanceHealthChecks);

                await dbContext.SaveChangesAsync(
                    stoppingToken);

                /*
                * INSTANCE DOWN ALERT
                */

                foreach (var instanceCheck in instanceHealthChecks)
                {
                    var activeInstanceDownAlert = await dbContext.Alerts
                        .FirstOrDefaultAsync(
                            a => a.ServiceName == service.Name
                                 && a.InstanceId == instanceCheck.InstanceId
                                 && a.Type == "INSTANCE_DOWN"
                                 && a.Status == "ACTIVE",
                            stoppingToken);

                    if (instanceCheck.Status == "DOWN")
                    {
                        if (activeInstanceDownAlert is null)
                        {
                            dbContext.Alerts.Add(new Alert
                            {
                                ServiceName = service.Name,
                                InstanceId = instanceCheck.InstanceId,
                                Type = "INSTANCE_DOWN",
                                Severity = "CRITICAL",
                                Message =
                                    $"{service.Name} instance " +
                                    $"{instanceCheck.InstanceId} is unavailable.",
                                Status = "ACTIVE",
                                TriggeredAt = DateTime.UtcNow
                            });

                            await dbContext.SaveChangesAsync(
                                stoppingToken);
                        }
                    }
                    else if (instanceCheck.Status == "UP")
                    {
                        if (activeInstanceDownAlert is not null)
                        {
                            activeInstanceDownAlert.Status = "RESOLVED";
                            activeInstanceDownAlert.ResolvedAt = DateTime.UtcNow;

                            await dbContext.SaveChangesAsync(
                                stoppingToken);
                        }
                    }
                }

                /*
                * LOW INSTANCE COUNT ALERT
                */

                var activeLowInstanceCountAlert = await dbContext.Alerts
                    .FirstOrDefaultAsync(
                        a => a.ServiceName == service.Name
                             && a.Type == "LOW_INSTANCE_COUNT"
                             && a.Status == "ACTIVE",
                        stoppingToken);

                if (activeInstanceCount < service.MinimumActiveInstances)
                {
                    if (activeLowInstanceCountAlert is null)
                    {
                        dbContext.Alerts.Add(new Alert
                        {
                            ServiceName = service.Name,
                            Type = "LOW_INSTANCE_COUNT",
                            Severity = "WARNING",
                            Message =
                                $"{service.Name} has {activeInstanceCount} active instance(s), " +
                                $"but the minimum expected is {service.MinimumActiveInstances}.",
                            Status = "ACTIVE",
                            TriggeredAt = DateTime.UtcNow
                        });

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                else
                {
                    if (activeLowInstanceCountAlert is not null)
                    {
                        activeLowInstanceCountAlert.Status = "RESOLVED";
                        activeLowInstanceCountAlert.ResolvedAt = DateTime.UtcNow;

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }

                /*
                 * HIGH RESPONSE TIME ALERT
                 */

                var activeResponseTimeAlert =
                    await dbContext.Alerts
                        .FirstOrDefaultAsync(
                            a =>
                                a.ServiceName == service.Name
                                && a.Type == "HIGH_RESPONSE_TIME"
                                && a.Status == "ACTIVE",
                            stoppingToken);

                if (currentStatus == "UP"
                    && responseTimeMs.HasValue
                    && responseTimeMs.Value
                    > service.ResponseTimeThresholdMs)
                {
                    if (activeResponseTimeAlert is null)
                    {
                        dbContext.Alerts.Add(
                            new Alert
                            {
                                ServiceName = service.Name,
                                Type = "HIGH_RESPONSE_TIME",
                                Severity = "WARNING",
                                Message =
                                    $"{service.Name} response time is " +
                                    $"{responseTimeMs.Value:F2} ms, " +
                                    $"which exceeds the threshold of " +
                                    $"{service.ResponseTimeThresholdMs:F0} ms.",
                                Status = "ACTIVE",
                                TriggeredAt = DateTime.UtcNow
                            });

                        await dbContext.SaveChangesAsync(
                            stoppingToken);
                    }
                }
                else
                {
                    if (activeResponseTimeAlert is not null)
                    {
                        activeResponseTimeAlert.Status =
                            "RESOLVED";

                        activeResponseTimeAlert.ResolvedAt =
                            DateTime.UtcNow;

                        await dbContext.SaveChangesAsync(
                            stoppingToken);
                    }
                }

                /*
                 * SERVICE DOWN ALERT
                 */

                _previousStatuses.TryGetValue(
                    service.Name,
                    out var previousStatus);

                if (previousStatus != currentStatus)
                {
                    _logger.LogInformation(
                        "{ServiceName} status changed: {PreviousStatus} -> {CurrentStatus}",
                        service.Name,
                        previousStatus ?? "UNKNOWN",
                        currentStatus);

                    if (currentStatus == "DOWN")
                    {
                        var existingAlert =
                            await dbContext.Alerts
                                .FirstOrDefaultAsync(
                                    a =>
                                        a.ServiceName
                                            == service.Name
                                        && a.Type
                                            == "SERVICE_DOWN"
                                        && a.Status
                                            == "ACTIVE",
                                    stoppingToken);

                        if (existingAlert is null)
                        {
                            dbContext.Alerts.Add(
                                new Alert
                                {
                                    ServiceName = service.Name,
                                    Type = "SERVICE_DOWN",
                                    Severity = "CRITICAL",
                                    Message =
                                        $"{service.Name} service is unavailable.",
                                    Status = "ACTIVE",
                                    TriggeredAt =
                                        DateTime.UtcNow
                                });

                            await dbContext.SaveChangesAsync(
                                stoppingToken);
                        }
                    }
                    else if (currentStatus == "UP")
                    {
                        var activeServiceDownAlert =
                            await dbContext.Alerts
                                .FirstOrDefaultAsync(
                                    a =>
                                        a.ServiceName
                                            == service.Name
                                        && a.Type
                                            == "SERVICE_DOWN"
                                        && a.Status
                                            == "ACTIVE",
                                    stoppingToken);

                        if (activeServiceDownAlert is not null)
                        {
                            activeServiceDownAlert.Status =
                                "RESOLVED";

                            activeServiceDownAlert.ResolvedAt =
                                DateTime.UtcNow;

                            await dbContext.SaveChangesAsync(
                                stoppingToken);
                        }
                    }

                    _previousStatuses[service.Name] =
                        currentStatus;
                }
            }

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }
}