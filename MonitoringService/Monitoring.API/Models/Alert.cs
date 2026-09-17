namespace Monitoring.API.Models;

public class Alert
{
    public long Id { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string? InstanceId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = "ACTIVE";

    public DateTime TriggeredAt { get; set; }

    public DateTime? ResolvedAt { get; set; }
}