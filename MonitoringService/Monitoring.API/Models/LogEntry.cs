namespace Monitoring.API.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Stream { get; set; } = string.Empty;

    public string ContainerId { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public string InstanceId { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public int? StatusCode { get; set; }

    public double? LatencyMs { get; set; }

    public string? Exception { get; set; }
}