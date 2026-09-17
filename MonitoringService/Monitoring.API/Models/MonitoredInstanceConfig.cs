namespace Monitoring.API.Models;

public class MonitoredInstanceConfig
{
    public string InstanceId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthEndpoint { get; set; } = "/health";
}