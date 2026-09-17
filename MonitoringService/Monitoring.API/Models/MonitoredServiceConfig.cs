namespace Monitoring.API.Models;

public class MonitoredServiceConfig
{
    public string Name { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string HealthEndpoint { get; set; } = "/health";

    public double ResponseTimeThresholdMs { get; set; } = 1000;

    public int MinimumActiveInstances { get; set; } = 1;

    public List<MonitoredInstanceConfig> Instances { get; set; } = new();
}