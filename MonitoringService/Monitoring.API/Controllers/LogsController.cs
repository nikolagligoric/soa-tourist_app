using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Monitoring.API.Models;

namespace Monitoring.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LogsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
    [FromQuery] string? service = null,
    [FromQuery] string? level = null,
    [FromQuery] DateTimeOffset? from = null,
    [FromQuery] DateTimeOffset? to = null,
    [FromQuery] string? search = null,
    [FromQuery] string? instance = null,
    [FromQuery] string? correlationId = null,
    [FromQuery] int limit = 50,
    [FromQuery] DateTimeOffset? before = null)
    {
        var client = _httpClientFactory.CreateClient("Loki");

        limit = Math.Clamp(limit, 1, 200);

        var lokiQuery = string.IsNullOrWhiteSpace(service)
            ? "{job=\"fluentbit\",service=~\".+\"}"
            : $"{{job=\"fluentbit\",service=\"{service}\"}}";

        if (!string.IsNullOrWhiteSpace(level) || !string.IsNullOrWhiteSpace(instance) || !string.IsNullOrWhiteSpace(correlationId))
        {
            lokiQuery += " | json";
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            lokiQuery += $" | level=\"{level}\"";
        }

        if (!string.IsNullOrWhiteSpace(instance))
        {
            var escapedInstance = instance
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");

            lokiQuery +=
                $" | instanceId=\"{escapedInstance}\"";
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            var escapedCorrelationId = correlationId
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");

            lokiQuery +=
                $" | correlationId=\"{escapedCorrelationId}\"";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escapedSearch = search
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");

            lokiQuery += $" |= \"{escapedSearch}\"";
        }

        var query = Uri.EscapeDataString(lokiQuery);

        var requestUrl = $"/loki/api/v1/query_range?query={query}&limit={limit}&direction=backward";

        if (from.HasValue)
        {
            var startNanoseconds =
                from.Value.ToUnixTimeMilliseconds() * 1_000_000;

            requestUrl += $"&start={startNanoseconds}";
        }

        DateTimeOffset? endTime = to;

        if (before.HasValue)
        {
            var beforeTime = before.Value.AddTicks(-1);

            if (!endTime.HasValue || beforeTime < endTime.Value)
            {
                endTime = beforeTime;
            }
        }

        if (endTime.HasValue)
        {
            var endNanoseconds =
                endTime.Value.ToUnixTimeMilliseconds() * 1_000_000;

            requestUrl += $"&end={endNanoseconds}";
        }

        var response = await client.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            return StatusCode(
                (int)response.StatusCode,
                error);
        }

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        var logs = new List<LogEntry>();

        var results = document.RootElement
            .GetProperty("data")
            .GetProperty("result");

        foreach (var result in results.EnumerateArray())
        {
            var values = result.GetProperty("values");

            foreach (var value in values.EnumerateArray())
            {
                var rawLog = value[1].GetString();

                if (string.IsNullOrWhiteSpace(rawLog))
                {
                    continue;
                }

                using var logDocument = JsonDocument.Parse(rawLog);

                var logRoot = logDocument.RootElement;

                var message = logRoot.TryGetProperty("log", out var logProperty)
                    ? logProperty.GetString() ?? string.Empty
                    : rawLog;

                var serviceName = string.Empty;
                var logInstanceId = string.Empty;
                var logLevel = string.Empty;
                var logCorrelationId = string.Empty;
                var method = string.Empty;
                var path = string.Empty;
                int? statusCode = null;
                double? latencyMs = null;
                DateTime? structuredTimestamp = null;
                string? logException = null;

                if (!string.IsNullOrWhiteSpace(message))
                {
                    try
                    {
                        using var structuredLogDocument =
                            JsonDocument.Parse(message);

                        var structuredLog =
                            structuredLogDocument.RootElement;

                        if (structuredLog.TryGetProperty(
                                "timestamp",
                                out var timestampProperty) &&
                                DateTime.TryParse(
                                timestampProperty.GetString(),
                                out var parsedStructuredTimestamp))
                        {
                            structuredTimestamp = parsedStructuredTimestamp;
                        }

                        if (structuredLog.TryGetProperty(
                                "serviceName",
                                out var serviceNameProperty))
                        {
                            serviceName =
                                serviceNameProperty.GetString()
                                ?? string.Empty;
                        }

                        if (structuredLog.TryGetProperty(
                                "instanceId",
                                out var instanceIdProperty))
                        {
                            logInstanceId =
                                instanceIdProperty.GetString()
                                ?? string.Empty;
                        }

                        if (structuredLog.TryGetProperty(
                                "level",
                                out var levelProperty))
                        {
                            logLevel =
                                levelProperty.GetString()
                                ?? string.Empty;
                        }

                        if (structuredLog.TryGetProperty(
                                "correlationId",
                                out var correlationIdProperty))
                        {
                            logCorrelationId =
                                correlationIdProperty.GetString()
                                ?? string.Empty;
                        }

                        if (structuredLog.TryGetProperty(
                                "method",
                                out var methodProperty))
                        {
                            method =
                                methodProperty.GetString()
                                ?? string.Empty;
                        }

                        if (structuredLog.TryGetProperty(
                                "path",
                                out var pathProperty))
                        {
                            path =
                                pathProperty.GetString()
                                ?? string.Empty;
                        }

                        if (structuredLog.TryGetProperty(
                                "statusCode",
                                out var statusCodeProperty) &&
                            statusCodeProperty.TryGetInt32(out var parsedStatusCode))
                        {
                            statusCode = parsedStatusCode;
                        }

                        if (structuredLog.TryGetProperty(
                                "latencyMs",
                                out var latencyProperty) &&
                            latencyProperty.TryGetDouble(out var parsedLatency))
                        {
                            latencyMs = parsedLatency;
                        }

                        if (structuredLog.TryGetProperty(
                                "message",
                                out var messageProperty))
                        {
                            message =
                                messageProperty.GetString()
                                ?? message;
                        }
                        if (structuredLog.TryGetProperty(
                            "exception",
                            out var exceptionProperty))
                        {
                            logException =
                                exceptionProperty.ValueKind == JsonValueKind.Null
                                    ? null
                                    : exceptionProperty.GetString();
                        }
                    }
                    catch (JsonException)
                    {
                        
                    }
                }

                var stream = logRoot.TryGetProperty(
                        "stream",
                        out var streamProperty)
                    ? streamProperty.GetString() ?? string.Empty
                    : string.Empty;

                var containerPath =
                    logRoot.TryGetProperty(
                        "container_path",
                        out var containerPathProperty)
                        ? containerPathProperty.GetString()
                          ?? string.Empty
                        : string.Empty;

                var containerId = string.Empty;

                if (!string.IsNullOrWhiteSpace(containerPath))
                {
                    var parts = containerPath.Split(
                        '/',
                        StringSplitOptions.RemoveEmptyEntries);

                    var containersIndex =
                        Array.IndexOf(parts, "containers");

                    if (containersIndex >= 0 &&
                        containersIndex + 1 < parts.Length)
                    {
                        containerId =
                            parts[containersIndex + 1];
                    }
                }

                DateTime timestamp = DateTime.UtcNow;

                if (logRoot.TryGetProperty(
                        "time",
                        out var timeProperty))
                {
                    DateTime.TryParse(
                        timeProperty.GetString(),
                        out timestamp);
                }

                logs.Add(new LogEntry
                {
                    Timestamp = structuredTimestamp ?? timestamp,
                    Message = message.Trim(),
                    Stream = stream,
                    ContainerId = containerId,
                    ServiceName = serviceName,
                    InstanceId = !string.IsNullOrWhiteSpace(logInstanceId)
                        ? logInstanceId
                        : containerId,
                    Level = logLevel,
                    CorrelationId = logCorrelationId,
                    Method = method,
                    Path = path,
                    StatusCode = statusCode,
                    LatencyMs = latencyMs,
                    Exception = logException
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            return Ok(logs.OrderBy(log => log.Timestamp));
        }

        return Ok(logs.OrderByDescending(log => log.Timestamp));
    }
}