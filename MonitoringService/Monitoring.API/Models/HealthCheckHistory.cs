namespace Monitoring.API.Models;

public class HealthCheckHistory
{
    public long Id { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public double? ResponseTimeMs { get; set; }

    public DateTime CheckedAt { get; set; }
}