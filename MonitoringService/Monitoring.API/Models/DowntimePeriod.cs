namespace Monitoring.API.Models;

public class DowntimePeriod
{
    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public double DurationSeconds { get; set; }

    public bool IsOngoing { get; set; }
}