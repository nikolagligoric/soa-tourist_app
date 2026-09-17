using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Monitoring.API.Models;
using Microsoft.EntityFrameworkCore;
using Monitoring.API.Data;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<MonitoredServiceConfig> _monitoredServices;
    private readonly MonitoringDbContext _dbContext;

    public DashboardController(IHttpClientFactory httpClientFactory, List<MonitoredServiceConfig> monitoredServices, MonitoringDbContext dbContext)
    {
        _httpClientFactory = httpClientFactory;
        _monitoredServices = monitoredServices;
        _dbContext = dbContext;
    }

    [HttpGet("log-summary")]
    public async Task<IActionResult> GetLogSummary(
        [FromQuery] int hours = 24,
        CancellationToken cancellationToken = default)
    {
        if (hours < 1 || hours > 168)
        {
            return BadRequest(new
            {
                message =
                    "Hours must be between 1 and 168."
            });
        }

        var range = $"{hours}h";

        var totalLogsQuery =
            $"sum(count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}}" +
            $"[{range}]))";

        var errorLogsQuery =
            $"sum(count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}} " +
            $"| json | level=\"ERROR\" " +
            $"[{range}]))";

        var warningLogsQuery =
            $"sum(count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}} " +
            $"| json | level=\"WARNING\" " +
            $"[{range}]))";

        try
        {
            var totalLogsTask =
                GetCountAsync(
                    totalLogsQuery,
                    cancellationToken);

            var errorLogsTask =
                GetCountAsync(
                    errorLogsQuery,
                    cancellationToken);

            var warningLogsTask =
                GetCountAsync(
                    warningLogsQuery,
                    cancellationToken);

            await Task.WhenAll(
                totalLogsTask,
                errorLogsTask,
                warningLogsTask);

            return Ok(new
            {
                periodHours = hours,
                totalLogs = await totalLogsTask,
                errorLogs = await errorLogsTask,
                warningLogs = await warningLogsTask
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message =
                        "Failed to retrieve log statistics from Loki.",
                    error = ex.Message
                });
        }
    }

    private async Task<long> GetCountAsync(
        string lokiQuery,
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("Loki");

        var encodedQuery =
            Uri.EscapeDataString(lokiQuery);

        var response = await client.GetAsync(
            $"/loki/api/v1/query?query={encodedQuery}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var content =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        using var document =
            JsonDocument.Parse(content);

        var results = document.RootElement
            .GetProperty("data")
            .GetProperty("result");

        if (results.GetArrayLength() == 0)
        {
            return 0;
        }

        var value = results[0]
            .GetProperty("value")[1]
            .GetString();

        if (double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var count))
        {
            return (long)Math.Round(count);
        }

        return 0;
    }

    [HttpGet("logs-by-service")]
    public async Task<IActionResult> GetLogsByService(
    [FromQuery] int hours = 24,
    CancellationToken cancellationToken = default)
    {
        if (hours < 1 || hours > 168)
        {
            return BadRequest(new
            {
                message = "Hours must be between 1 and 168."
            });
        }

        var range = $"{hours}h";

        var totalQuery =
            $"sum by (service) (count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}}" +
            $"[{range}]))";

        var errorQuery =
            $"sum by (service) (count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}} " +
            $"| json | level=\"ERROR\" " +
            $"[{range}]))";

        var warningQuery =
            $"sum by (service) (count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}} " +
            $"| json | level=\"WARNING\" " +
            $"[{range}]))";

        try
        {
            var totalTask =
                GetCountsByServiceAsync(
                    totalQuery,
                    cancellationToken);

            var errorTask =
                GetCountsByServiceAsync(
                    errorQuery,
                    cancellationToken);

            var warningTask =
                GetCountsByServiceAsync(
                    warningQuery,
                    cancellationToken);

            await Task.WhenAll(
                totalTask,
                errorTask,
                warningTask);

            var totalCounts = await totalTask;
            var errorCounts = await errorTask;
            var warningCounts = await warningTask;

            var services = _monitoredServices
                .Select(service =>
                {
                    totalCounts.TryGetValue(
                        service.Name,
                        out var totalLogs);

                    errorCounts.TryGetValue(
                        service.Name,
                        out var errorLogs);

                    warningCounts.TryGetValue(
                        service.Name,
                        out var warningLogs);

                    return new
                    {
                        serviceName = service.Name,
                        totalLogs,
                        errorLogs,
                        warningLogs
                    };
                })
                .OrderByDescending(service => service.errorLogs)
                .ThenByDescending(service => service.warningLogs)
                .ToList();

            return Ok(new
            {
                periodHours = hours,
                services
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message =
                        "Failed to retrieve log statistics from Loki.",
                    error = ex.Message
                });
        }
    }

    [HttpGet("log-trend")]
    public async Task<IActionResult> GetLogTrend(
    [FromQuery] int hours = 24,
    CancellationToken cancellationToken = default)
    {
        if (hours < 1 || hours > 168)
        {
            return BadRequest(new
            {
                message = "Hours must be between 1 and 168."
            });
        }

        string bucket;

        if (hours <= 6)
        {
            bucket = "5m";
        }
        else if (hours <= 24)
        {
            bucket = "30m";
        }
        else if (hours <= 72)
        {
            bucket = "1h";
        }
        else
        {
            bucket = "6h";
        }

        var totalQuery =
            $"sum(count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}}" +
            $"[{bucket}]))";

        var errorQuery =
            $"sum(count_over_time(" +
            $"{{job=\"fluentbit\",service=~\".+\"}} " +
            $"| json | level=\"ERROR\" " +
            $"[{bucket}]))";

        try
        {
            var to = DateTimeOffset.UtcNow;
            var from = to.AddHours(-hours);

            var totalTask = GetTimeSeriesAsync(
                totalQuery,
                from,
                to,
                bucket,
                cancellationToken);

            var errorTask = GetTimeSeriesAsync(
                errorQuery,
                from,
                to,
                bucket,
                cancellationToken);

            await Task.WhenAll(
                totalTask,
                errorTask);

            var totalSeries = await totalTask;
            var errorSeries = await errorTask;

            var currentTimestamp = to.ToUnixTimeSeconds();

            var timestamps = totalSeries.Keys
                .Union(errorSeries.Keys)
                .Where(timestamp => timestamp <= currentTimestamp)
                .OrderBy(timestamp => timestamp)
                .ToList();

            var points = timestamps
                .Select(timestamp => new
                {
                    timestamp =
                        DateTimeOffset
                            .FromUnixTimeSeconds(timestamp)
                            .UtcDateTime,

                    totalLogs =
                        totalSeries.TryGetValue(
                            timestamp,
                            out var total)
                                ? total
                                : 0,

                    errorLogs =
                        errorSeries.TryGetValue(
                            timestamp,
                            out var errors)
                                ? errors
                                : 0
                })
                .ToList();

            return Ok(new
            {
                periodHours = hours,
                bucket,
                points
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message =
                        "Failed to retrieve log trend from Loki.",
                    error = ex.Message
                });
        }
    }

    [HttpGet("response-time-by-service")]
    public async Task<IActionResult> GetResponseTimeByService(
    [FromQuery] int hours = 24,
    CancellationToken cancellationToken = default)
    {
        if (hours < 1 || hours > 168)
        {
            return BadRequest(new
            {
                message = "Hours must be between 1 and 168."
            });
        }

        var from = DateTime.UtcNow.AddHours(-hours);

        var statistics = await _dbContext.HealthCheckHistories
            .Where(h =>
                h.CheckedAt >= from &&
                h.ResponseTimeMs.HasValue)
            .GroupBy(h => h.ServiceName)
            .Select(group => new
            {
                ServiceName = group.Key,
                Average = group.Average(
                    h => h.ResponseTimeMs!.Value),
                Minimum = group.Min(
                    h => h.ResponseTimeMs!.Value),
                Maximum = group.Max(
                    h => h.ResponseTimeMs!.Value)
            })
            .ToListAsync(cancellationToken);

        var services = _monitoredServices
            .Select(service =>
            {
                var statistic = statistics
                    .FirstOrDefault(s =>
                        s.ServiceName == service.Name);

                return new
                {
                    serviceName = service.Name,

                    averageResponseTimeMs =
                        statistic is null
                            ? (double?)null
                            : Math.Round(
                                statistic.Average,
                                2),

                    minimumResponseTimeMs =
                        statistic is null
                            ? (double?)null
                            : Math.Round(
                                statistic.Minimum,
                                2),

                    maximumResponseTimeMs =
                        statistic is null
                            ? (double?)null
                            : Math.Round(
                                statistic.Maximum,
                                2)
                };
            })
            .ToList();

        return Ok(new
        {
            periodHours = hours,
            services
        });
    }

    [HttpGet("availability-by-service")]
    public async Task<IActionResult> GetAvailabilityByService(
    [FromQuery] int hours = 24,
    CancellationToken cancellationToken = default)
    {
        if (hours < 1 || hours > 168)
        {
            return BadRequest(new
            {
                message = "Hours must be between 1 and 168."
            });
        }

        var from = DateTime.UtcNow.AddHours(-hours);

        var statistics = await _dbContext.HealthCheckHistories
            .Where(h => h.CheckedAt >= from)
            .GroupBy(h => h.ServiceName)
            .Select(group => new
            {
                ServiceName = group.Key,
                TotalChecks = group.Count(),
                UpChecks = group.Count(h => h.Status == "UP")
            })
            .ToListAsync(cancellationToken);

        var services = _monitoredServices
            .Select(service =>
            {
                var statistic = statistics
                    .FirstOrDefault(s =>
                        s.ServiceName == service.Name);

                if (statistic is null ||
                    statistic.TotalChecks == 0)
                {
                    return new
                    {
                        serviceName = service.Name,
                        totalChecks = 0,
                        upChecks = 0,
                        downChecks = 0,
                        availabilityPercentage =
                            (double?)null
                    };
                }

                var downChecks =
                    statistic.TotalChecks -
                    statistic.UpChecks;

                var availabilityPercentage =
                    (double)statistic.UpChecks /
                    statistic.TotalChecks * 100;

                return new
                {
                    serviceName = service.Name,
                    totalChecks = statistic.TotalChecks,
                    upChecks = statistic.UpChecks,
                    downChecks,
                    availabilityPercentage =
                        (double?)Math.Round(
                            availabilityPercentage,
                            2)
                };
            })
            .ToList();

        return Ok(new
        {
            periodHours = hours,
            services
        });
    }

    private async Task<Dictionary<string, long>>
    GetCountsByServiceAsync(
        string lokiQuery,
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("Loki");

        var encodedQuery =
            Uri.EscapeDataString(lokiQuery);

        var response = await client.GetAsync(
            $"/loki/api/v1/query?query={encodedQuery}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var content =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        using var document =
            JsonDocument.Parse(content);

        var results = document.RootElement
            .GetProperty("data")
            .GetProperty("result");

        var counts =
            new Dictionary<string, long>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var result in results.EnumerateArray())
        {
            var metric = result.GetProperty("metric");

            if (!metric.TryGetProperty(
                    "service",
                    out var serviceProperty))
            {
                continue;
            }

            var serviceName =
                serviceProperty.GetString();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                continue;
            }

            var value = result
                .GetProperty("value")[1]
                .GetString();

            if (double.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var count))
            {
                counts[serviceName] =
                    (long)Math.Round(count);
            }
        }

        return counts;
    }

    private async Task<Dictionary<long, long>>
    GetTimeSeriesAsync(
        string lokiQuery,
        DateTimeOffset from,
        DateTimeOffset to,
        string step,
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("Loki");

        var query =
            Uri.EscapeDataString(lokiQuery);

        var start =
            from.ToUnixTimeMilliseconds() * 1_000_000;

        var end =
            to.ToUnixTimeMilliseconds() * 1_000_000;

        var response = await client.GetAsync(
            $"/loki/api/v1/query_range" +
            $"?query={query}" +
            $"&start={start}" +
            $"&end={end}" +
            $"&step={step}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var content =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        using var document =
            JsonDocument.Parse(content);

        var results = document.RootElement
            .GetProperty("data")
            .GetProperty("result");

        var values =
            new Dictionary<long, long>();

        foreach (var result in results.EnumerateArray())
        {
            if (!result.TryGetProperty(
                    "values",
                    out var seriesValues))
            {
                continue;
            }

            foreach (var value in seriesValues.EnumerateArray())
            {
                var timestamp =
                    (long)value[0].GetDouble();

                var rawValue =
                    value[1].GetString();

                if (double.TryParse(
                        rawValue,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var parsedValue))
                {
                    values[timestamp] =
                        (long)Math.Round(parsedValue);
                }
            }
        }

        return values;
    }
}

