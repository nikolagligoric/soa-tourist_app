using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Monitoring.API.Data;
using Monitoring.API.Models;

namespace Monitoring.API.BackgroundServices;

public class LogMonitoringBackgroundService : BackgroundService
{
    private const int ErrorThreshold = 3;
    private const int FailedLoginThreshold = 3;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<MonitoredServiceConfig> _monitoredServices;
    private readonly ILogger<LogMonitoringBackgroundService> _logger;

    public LogMonitoringBackgroundService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        List<MonitoredServiceConfig> monitoredServices,
        ILogger<LogMonitoringBackgroundService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _monitoredServices = monitoredServices;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var service in _monitoredServices)
            {
                try
                {
                    var errorCount = await GetErrorCountAsync(
                        service.Name,
                        stoppingToken);

                    if (!errorCount.HasValue)
                    {
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();

                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<MonitoringDbContext>();

                    var activeAlert = await dbContext.Alerts
                        .FirstOrDefaultAsync(
                            a => a.ServiceName == service.Name
                                 && a.Type == "HIGH_ERROR_RATE"
                                 && a.Status == "ACTIVE",
                            stoppingToken);

                    if (errorCount.Value >= ErrorThreshold)
                    {
                        if (activeAlert is null)
                        {
                            dbContext.Alerts.Add(
                                new Alert
                                {
                                    ServiceName = service.Name,
                                    Type = "HIGH_ERROR_RATE",
                                    Severity = "WARNING",
                                    Message =
                                        $"{service.Name} generated " +
                                        $"{errorCount.Value} ERROR logs " +
                                        $"in the last minute. " +
                                        $"Threshold is {ErrorThreshold}.",
                                    Status = "ACTIVE",
                                    TriggeredAt = DateTime.UtcNow
                                });

                            await dbContext.SaveChangesAsync(
                                stoppingToken);

                            _logger.LogWarning(
                                "HIGH_ERROR_RATE alert created for {ServiceName}. Error count: {ErrorCount}",
                                service.Name,
                                errorCount.Value);
                        }
                    }
                    else
                    {
                        if (activeAlert is not null)
                        {
                            activeAlert.Status = "RESOLVED";
                            activeAlert.ResolvedAt = DateTime.UtcNow;

                            await dbContext.SaveChangesAsync(
                                stoppingToken);

                            _logger.LogInformation(
                                "HIGH_ERROR_RATE alert resolved for {ServiceName}.",
                                service.Name);
                        }
                    }
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while checking ERROR logs for {ServiceName}.",
                        service.Name);
                }
            }

            try
            {
                var failedLoginCount =
                    await GetFailedLoginCountAsync(
                        stoppingToken);

                if (failedLoginCount.HasValue)
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<MonitoringDbContext>();

                    var activeLoginAlert =
                        await dbContext.Alerts
                            .FirstOrDefaultAsync(
                                a =>
                                    a.ServiceName == "Gateway"
                                    && a.Type == "FAILED_LOGIN_ATTEMPTS"
                                    && a.Status == "ACTIVE",
                                stoppingToken);

                    if (failedLoginCount.Value >= FailedLoginThreshold)
                    {
                        if (activeLoginAlert is null)
                        {
                            dbContext.Alerts.Add(
                                new Alert
                                {
                                    ServiceName = "Gateway",
                                    Type = "FAILED_LOGIN_ATTEMPTS",
                                    Severity = "WARNING",
                                    Message =
                                        $"{failedLoginCount.Value} failed login attempts " +
                                        $"were detected in the last minute. " +
                                        $"Threshold is {FailedLoginThreshold}.",
                                    Status = "ACTIVE",
                                    TriggeredAt = DateTime.UtcNow
                                });

                            await dbContext.SaveChangesAsync(
                                stoppingToken);

                            _logger.LogWarning(
                                "FAILED_LOGIN_ATTEMPTS alert created. Count: {FailedLoginCount}",
                                failedLoginCount.Value);
                        }
                    }
                    else
                    {
                        if (activeLoginAlert is not null)
                        {
                            activeLoginAlert.Status =
                                "RESOLVED";

                            activeLoginAlert.ResolvedAt =
                                DateTime.UtcNow;

                            await dbContext.SaveChangesAsync(
                                stoppingToken);

                            _logger.LogInformation(
                                "FAILED_LOGIN_ATTEMPTS alert resolved.");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while checking failed login attempts.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }

    private async Task<int?> GetErrorCountAsync(
        string serviceName,
        CancellationToken stoppingToken)
    {
        var client =
            _httpClientFactory.CreateClient("Loki");

        var escapedServiceName =
            serviceName
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");

        var lokiQuery =
            $"sum(count_over_time(" +
            $"{{job=\"fluentbit\",service=\"{escapedServiceName}\"}} " +
            $"| json | level=\"ERROR\" [1m]))";

        return await ExecuteCountQueryAsync(
            client,
            lokiQuery,
            $"ERROR logs for {serviceName}",
            stoppingToken);
    }

    private async Task<int?> GetFailedLoginCountAsync(
        CancellationToken stoppingToken)
    {
        var client =
            _httpClientFactory.CreateClient("Loki");

        var lokiQuery =
            $"sum(count_over_time(" +
            $"{{job=\"fluentbit\",service=\"Gateway\"}} " +
            $"| json " +
            $"| level=\"ERROR\" " +
            $"| path=\"/api/users/login\" [1m]))";

        return await ExecuteCountQueryAsync(
            client,
            lokiQuery,
            "failed login attempts",
            stoppingToken);
    }

    private async Task<int?> ExecuteCountQueryAsync(
        HttpClient client,
        string lokiQuery,
        string queryDescription,
        CancellationToken stoppingToken)
    {
        var encodedQuery =
            Uri.EscapeDataString(lokiQuery);

        var response =
            await client.GetAsync(
                $"/loki/api/v1/query?query={encodedQuery}",
                stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    stoppingToken);

            _logger.LogWarning(
                "Loki query failed for {QueryDescription}: {Error}",
                queryDescription,
                error);

            return null;
        }

        var content =
            await response.Content.ReadAsStringAsync(
                stoppingToken);

        using var document =
            JsonDocument.Parse(content);

        var results =
            document.RootElement
                .GetProperty("data")
                .GetProperty("result");

        if (results.GetArrayLength() == 0)
        {
            return 0;
        }

        var value =
            results[0]
                .GetProperty("value")[1]
                .GetString();

        if (double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var count))
        {
            return (int)count;
        }

        return 0;
    }
}