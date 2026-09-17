using System.Collections.Concurrent;
using Monitoring.API.Models;

namespace Monitoring.API.Services;

public class ServiceStatusStore
{
    private readonly ConcurrentDictionary<string, ServiceHealthStatus>
        _statuses = new();

    private readonly ConcurrentDictionary<string, InstanceHealthStatus>
        _instanceStatuses = new();

    public void SetStatus(ServiceHealthStatus status)
    {
        _statuses[status.ServiceName] = status;
    }

    public ServiceHealthStatus? GetStatus(string serviceName)
    {
        _statuses.TryGetValue(serviceName, out var status);
        return status;
    }

    public IEnumerable<ServiceHealthStatus> GetAllStatuses()
    {
        return _statuses.Values;
    }

    public void SetInstanceStatus(InstanceHealthStatus status)
    {
        var key = GetInstanceKey(
            status.ServiceName,
            status.InstanceId);

        _instanceStatuses[key] = status;
    }

    public InstanceHealthStatus? GetInstanceStatus(
        string serviceName,
        string instanceId)
    {
        var key = GetInstanceKey(
            serviceName,
            instanceId);

        _instanceStatuses.TryGetValue(
            key,
            out var status);

        return status;
    }

    public IEnumerable<InstanceHealthStatus> GetInstanceStatuses(
        string serviceName)
    {
        return _instanceStatuses.Values
            .Where(status =>
                status.ServiceName.Equals(
                    serviceName,
                    StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<InstanceHealthStatus> GetAllInstanceStatuses()
    {
        return _instanceStatuses.Values;
    }

    private static string GetInstanceKey(
        string serviceName,
        string instanceId)
    {
        return $"{serviceName}:{instanceId}";
    }
}