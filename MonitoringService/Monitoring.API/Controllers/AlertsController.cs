using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monitoring.API.Data;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly MonitoringDbContext _dbContext;

    public AlertsController(MonitoringDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = await _dbContext.Alerts
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var alerts = await _dbContext.Alerts
            .Where(a => a.Status == "ACTIVE")
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpGet("service/{serviceName}")]
    public async Task<IActionResult> GetServiceAlerts(string serviceName)
    {
        var alerts = await _dbContext.Alerts
            .Where(a => a.ServiceName == serviceName)
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

        return Ok(alerts);
    }
}