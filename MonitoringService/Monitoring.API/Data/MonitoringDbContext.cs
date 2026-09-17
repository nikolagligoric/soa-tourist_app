using Microsoft.EntityFrameworkCore;
using Monitoring.API.Models;

namespace Monitoring.API.Data;

public class MonitoringDbContext : DbContext
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
        : base(options)
    {
    }

    public DbSet<HealthCheckHistory> HealthCheckHistories { get; set; }

    public DbSet<InstanceHealthCheckHistory> InstanceHealthCheckHistories { get; set; }

    public DbSet<Alert> Alerts { get; set; }
}