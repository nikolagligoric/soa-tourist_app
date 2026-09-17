using Microsoft.AspNetCore.Mvc;
using Monitoring.API.Services;
using Microsoft.EntityFrameworkCore;
using Monitoring.API.Data;
using Monitoring.API.Models;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ServiceStatusStore _statusStore;
    private readonly MonitoringDbContext _dbContext;
    private readonly List<MonitoredServiceConfig> _monitoredServices;

    public HealthController(ServiceStatusStore statusStore, MonitoringDbContext dbContext, List<MonitoredServiceConfig> monitoredServices)
    {
        _statusStore = statusStore;
        _dbContext = dbContext;
        _monitoredServices = monitoredServices;
    }

    [HttpGet("{serviceName}")]
    public IActionResult GetServiceStatus(string serviceName)
    {
        var status = _statusStore.GetStatus(serviceName);

        if (status is null)
        {
            return NotFound(new
            {
                message = $"Service '{serviceName}' was not found."
            });
        }

        return Ok(status);
    }

    [HttpGet]
    public IActionResult GetAllStatuses()
    {
        var statuses = _statusStore.GetAllStatuses();

        return Ok(statuses);
    }

    [HttpGet("instances")]
    public IActionResult GetAllInstanceStatuses()
    {
        var instances = _statusStore.GetAllInstanceStatuses();

        return Ok(instances);
    }

    [HttpGet("{serviceName}/instances")]
    public IActionResult GetServiceInstances(string serviceName)
    {
        var instances = _statusStore
            .GetInstanceStatuses(serviceName)
            .ToList();

        if (instances.Count == 0)
        {
            return NotFound(new
            {
                message = $"No instances found for service '{serviceName}'."
            });
        }

        return Ok(instances);
    }

    [HttpGet("{serviceName}/instances/{instanceId}/history")]
    public async Task<IActionResult> GetInstanceHistory(
    string serviceName,
    string instanceId)
    {
        var history = await _dbContext.InstanceHealthCheckHistories
            .Where(h =>
                h.ServiceName == serviceName &&
                h.InstanceId == instanceId)
            .OrderByDescending(h => h.CheckedAt)
            .Take(50)
            .ToListAsync();

        if (history.Count == 0)
        {
            return NotFound(new
            {
                message =
                    $"No health history found for instance '{instanceId}' " +
                    $"of service '{serviceName}'."
            });
        }

        return Ok(history);
    }

    [HttpGet("{serviceName}/history")]
    public async Task<IActionResult> GetServiceHistory(string serviceName)
    {
        var history = await _dbContext.HealthCheckHistories
            .Where(h => h.ServiceName == serviceName)
            .OrderByDescending(h => h.CheckedAt)
            .Take(50)
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("{serviceName}/uptime")]
    public async Task<IActionResult> GetServiceUptime(string serviceName)
    {
        var checks = await _dbContext.HealthCheckHistories
            .Where(h => h.ServiceName == serviceName)
            .ToListAsync();

        if (checks.Count == 0)
        {
            return NotFound(new
            {
                message = $"No health history found for service '{serviceName}'."
            });
        }

        var upChecks = checks.Count(h => h.Status == "UP");

        var uptimePercentage =
            (double)upChecks / checks.Count * 100;

        return Ok(new
        {
            serviceName,
            totalChecks = checks.Count,
            upChecks,
            downChecks = checks.Count - upChecks,
            uptimePercentage = Math.Round(uptimePercentage, 2)
        });
    }

    [HttpGet("{serviceName}/response-time")]
    public async Task<IActionResult> GetAverageResponseTime(string serviceName)
    {
        var responseTimes = await _dbContext.HealthCheckHistories
            .Where(h =>
                h.ServiceName == serviceName &&
                h.ResponseTimeMs.HasValue)
            .Select(h => h.ResponseTimeMs!.Value)
            .ToListAsync();

        if (responseTimes.Count == 0)
        {
            return NotFound(new
            {
                message = $"No response time data found for service '{serviceName}'."
            });
        }

        return Ok(new
        {
            serviceName,
            averageResponseTimeMs = Math.Round(responseTimes.Average(), 2),
            minimumResponseTimeMs = Math.Round(responseTimes.Min(), 2),
            maximumResponseTimeMs = Math.Round(responseTimes.Max(), 2)
        });
    }

    [HttpGet("summary")]
    public IActionResult GetHealthSummary()
    {
        var services = _monitoredServices
            .Select(service =>
            {
                var instanceStatuses = _statusStore
                    .GetInstanceStatuses(service.Name)
                    .ToList();

                var activeInstances = instanceStatuses
                    .Count(instance => instance.Status == "UP");

                var serviceStatus =
                    _statusStore.GetStatus(service.Name);

                return new
                {
                    serviceName = service.Name,
                    status = serviceStatus?.Status ?? "UNKNOWN",
                    totalInstances = service.Instances.Count,
                    activeInstances,
                    minimumActiveInstances =
                        service.MinimumActiveInstances
                };
            })
            .ToList();

        return Ok(new
        {
            totalServices = _monitoredServices.Count,

            totalConfiguredInstances =
                _monitoredServices.Sum(
                    service => service.Instances.Count),

            totalActiveInstances =
                services.Sum(
                    service => service.activeInstances),

            services
        });
    }

    [HttpGet("{serviceName}/downtime")]
    public async Task<IActionResult> GetServiceDowntimePeriods(
    string serviceName)
    {
        var history = await _dbContext.HealthCheckHistories
            .Where(h => h.ServiceName == serviceName)
            .OrderBy(h => h.CheckedAt)
            .ToListAsync();

        if (history.Count == 0)
        {
            return NotFound(new
            {
                message =
                    $"No health history found for service '{serviceName}'."
            });
        }

        var downtimePeriods = new List<DowntimePeriod>();

        DateTime? downtimeStartedAt = null;

        foreach (var check in history)
        {
            if (check.Status == "DOWN" &&
                downtimeStartedAt is null)
            {
                downtimeStartedAt = check.CheckedAt;
            }
            else if (check.Status == "UP" &&
                     downtimeStartedAt.HasValue)
            {
                downtimePeriods.Add(
                    new DowntimePeriod
                    {
                        StartedAt = downtimeStartedAt.Value,
                        EndedAt = check.CheckedAt,
                        DurationSeconds =
                            (check.CheckedAt -
                             downtimeStartedAt.Value)
                            .TotalSeconds,
                        IsOngoing = false
                    });

                downtimeStartedAt = null;
            }
        }

        if (downtimeStartedAt.HasValue)
        {
            var now = DateTime.UtcNow;

            downtimePeriods.Add(
                new DowntimePeriod
                {
                    StartedAt = downtimeStartedAt.Value,
                    EndedAt = null,
                    DurationSeconds =
                        (now - downtimeStartedAt.Value)
                        .TotalSeconds,
                    IsOngoing = true
                });
        }

        return Ok(new
        {
            serviceName,
            downtimePeriods
        });
    }

    [HttpGet("{serviceName}/instances/{instanceId}/downtime")]
    public async Task<IActionResult> GetInstanceDowntimePeriods(
    string serviceName,
    string instanceId)
    {
        var history = await _dbContext.InstanceHealthCheckHistories
            .Where(h =>
                h.ServiceName == serviceName &&
                h.InstanceId == instanceId)
            .OrderBy(h => h.CheckedAt)
            .ToListAsync();

        if (history.Count == 0)
        {
            return NotFound(new
            {
                message =
                    $"No health history found for instance '{instanceId}' " +
                    $"of service '{serviceName}'."
            });
        }

        var downtimePeriods = new List<DowntimePeriod>();

        DateTime? downtimeStartedAt = null;

        foreach (var check in history)
        {
            if (check.Status == "DOWN" &&
                downtimeStartedAt is null)
            {
                downtimeStartedAt = check.CheckedAt;
            }
            else if (check.Status == "UP" &&
                     downtimeStartedAt.HasValue)
            {
                downtimePeriods.Add(
                    new DowntimePeriod
                    {
                        StartedAt = downtimeStartedAt.Value,
                        EndedAt = check.CheckedAt,
                        DurationSeconds =
                            (check.CheckedAt -
                             downtimeStartedAt.Value)
                            .TotalSeconds,
                        IsOngoing = false
                    });

                downtimeStartedAt = null;
            }
        }

        if (downtimeStartedAt.HasValue)
        {
            var now = DateTime.UtcNow;

            downtimePeriods.Add(
                new DowntimePeriod
                {
                    StartedAt = downtimeStartedAt.Value,
                    EndedAt = null,
                    DurationSeconds =
                        (now - downtimeStartedAt.Value)
                        .TotalSeconds,
                    IsOngoing = true
                });
        }

        return Ok(new
        {
            serviceName,
            instanceId,
            downtimePeriods
        });
    }
}