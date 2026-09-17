namespace Monitoring.API.Models;

public class ServiceHealthStatus
{
    public string ServiceName { get; set; } = string.Empty;

    public string Status { get; set; } = "UNKNOWN";

    public DateTime LastCheckedAt { get; set; }

    public DateTime? LastSuccessfulCheckAt { get; set; }

    public double? ResponseTimeMs { get; set; }
}