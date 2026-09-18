import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ServiceHealthSummary {
  serviceName: string;
  status: string;
  totalInstances: number;
  activeInstances: number;
  minimumActiveInstances: number;
}

export interface HealthSummary {
  totalServices: number;
  totalConfiguredInstances: number;
  totalActiveInstances: number;
  services: ServiceHealthSummary[];
}

export interface LogSummary {
  periodHours: number;
  totalLogs: number;
  errorLogs: number;
  warningLogs: number;
}

export interface ServiceLogStatistics {
  serviceName: string;
  totalLogs: number;
  errorLogs: number;
  warningLogs: number;
}

export interface LogsByService {
  periodHours: number;
  services: ServiceLogStatistics[];
}

export interface LogTrendPoint {
  timestamp: string;
  totalLogs: number;
  errorLogs: number;
}

export interface LogTrend {
  periodHours: number;
  bucket: string;
  points: LogTrendPoint[];
}

export interface ServiceResponseTime {
  serviceName: string;
  averageResponseTimeMs: number | null;
  minimumResponseTimeMs: number | null;
  maximumResponseTimeMs: number | null;
}

export interface ResponseTimeByService {
  periodHours: number;
  services: ServiceResponseTime[];
}

export interface ServiceAvailability {
  serviceName: string;
  totalChecks: number;
  upChecks: number;
  downChecks: number;
  availabilityPercentage: number | null;
}

export interface AvailabilityByService {
  periodHours: number;
  services: ServiceAvailability[];
}

export interface LogEntry {
  timestamp: string;
  message: string;
  stream: string;
  containerId: string;
  serviceName: string;
  instanceId: string;
  level: string;
  correlationId: string;
  method: string;
  path: string;
  statusCode: number | null;
  latencyMs: number | null;
  exception: string | null;
}

export interface LogFilters {
  service?: string;
  instance?: string;
  level?: string;
  from?: string;
  to?: string;
  search?: string;
  correlationId?: string;
  limit?: number;
  before?: string;
}

export interface Alert {
  id: number;
  serviceName: string;
  instanceId: string | null;
  type: string;
  severity: string;
  message: string;
  status: string;
  triggeredAt: string;
  resolvedAt: string | null;
}

export interface ServiceHealthStatus {
  serviceName: string;
  status: string;
  lastCheckedAt: string;
  lastSuccessfulCheckAt: string | null;
  responseTimeMs: number;
}

export interface InstanceHealthStatus {
  serviceName: string;
  instanceId: string;
  status: string;
  lastCheckedAt: string;
  lastSuccessfulCheckAt: string | null;
  responseTimeMs: number;
}

export interface HealthCheckHistory {
  id: number;
  serviceName: string;
  status: string;
  responseTimeMs: number | null;
  checkedAt: string;
}

export interface DowntimePeriod {
  startedAt: string;
  endedAt: string | null;
  durationSeconds: number;
  isOngoing: boolean;
}

export interface ServiceDowntime {
  serviceName: string;
  downtimePeriods: DowntimePeriod[];
}

export interface InstanceDowntime {
  serviceName: string;
  instanceId: string;
  downtimePeriods: DowntimePeriod[];
}

export interface ResponseTimeStats {
  averageResponseTimeMs: number | null;
  minimumResponseTimeMs: number | null;
  maximumResponseTimeMs: number | null;
}

export interface UptimeStats {
  uptimePercentage: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class MonitoringService {

  private readonly baseUrl = '/monitoring-api';

  constructor(private http: HttpClient) { }

  getHealthSummary(): Observable<HealthSummary> {
    return this.http.get<HealthSummary>(
      `${this.baseUrl}/health/summary`
    );
  }

  getLogSummary(hours: number = 24): Observable<LogSummary> {
    return this.http.get<LogSummary>(
      `${this.baseUrl}/dashboard/log-summary?hours=${hours}`
    );
  }

  getLogsByService(
    hours: number = 24
  ): Observable<LogsByService> {
    return this.http.get<LogsByService>(
      `${this.baseUrl}/dashboard/logs-by-service?hours=${hours}`
    );
  }

  getLogTrend(
    hours: number = 24
  ): Observable<LogTrend> {
    return this.http.get<LogTrend>(
      `${this.baseUrl}/dashboard/log-trend?hours=${hours}`
    );
  }

  getResponseTimeByService(
    hours: number = 24
  ): Observable<ResponseTimeByService> {
    return this.http.get<ResponseTimeByService>(
      `${this.baseUrl}/dashboard/response-time-by-service?hours=${hours}`
    );
  }

  getAvailabilityByService(
    hours: number = 24
  ): Observable<AvailabilityByService> {
    return this.http.get<AvailabilityByService>(
      `${this.baseUrl}/dashboard/availability-by-service?hours=${hours}`
    );
  }

  getLogs(filters: LogFilters = {}): Observable<LogEntry[]> {
    let params = new HttpParams();

    if (filters.service) {
      params = params.set('service', filters.service);
    }

    if (filters.instance) {
      params = params.set('instance', filters.instance);
    }

    if (filters.level) {
      params = params.set('level', filters.level);
    }

    if (filters.from) {
      params = params.set(
        'from',
        new Date(filters.from).toISOString()
      );
    }

    if (filters.to) {
      params = params.set(
        'to',
        new Date(filters.to).toISOString()
      );
    }

    if (filters.search) {
      params = params.set('search', filters.search);
    }

    if (filters.correlationId) {
      params = params.set(
        'correlationId',
        filters.correlationId
      );
    }

    if (filters.limit) {
      params = params.set(
        'limit',
        filters.limit.toString()
      );
    }

    if (filters.before) {
      params = params.set(
        'before',
        new Date(filters.before).toISOString()
      );
    }

    return this.http.get<LogEntry[]>(
      `${this.baseUrl}/logs`,
      { params }
    );
  }

  getAlerts(): Observable<Alert[]> {
    return this.http.get<Alert[]>(
      `${this.baseUrl}/alerts`
    );
  }

  getActiveAlerts(): Observable<Alert[]> {
    return this.http.get<Alert[]>(
      `${this.baseUrl}/alerts/active`
    );
  }

  getServiceHealth(
    serviceName: string
  ): Observable<ServiceHealthStatus> {
    return this.http.get<ServiceHealthStatus>(
      `${this.baseUrl}/health/${serviceName}`
    );
  }

  getServiceInstances(
    serviceName: string
  ): Observable<InstanceHealthStatus[]> {
    return this.http.get<InstanceHealthStatus[]>(
      `${this.baseUrl}/health/${serviceName}/instances`
    );
  }

  getServiceHistory(
    serviceName: string
  ): Observable<HealthCheckHistory[]> {
    return this.http.get<HealthCheckHistory[]>(
      `${this.baseUrl}/health/${serviceName}/history`
    );
  }

  getServiceDowntime(
    serviceName: string
  ): Observable<ServiceDowntime> {
    return this.http.get<ServiceDowntime>(
      `${this.baseUrl}/health/${serviceName}/downtime`
    );
  }

  getServiceResponseTime(
    serviceName: string
  ): Observable<ResponseTimeStats> {
    return this.http.get<ResponseTimeStats>(
      `${this.baseUrl}/health/${serviceName}/response-time`
    );
  }

  getServiceUptime(
    serviceName: string
  ): Observable<UptimeStats> {
    return this.http.get<UptimeStats>(
      `${this.baseUrl}/health/${serviceName}/uptime`
    );
  }

  getInstanceHistory(
    serviceName: string,
    instanceId: string
  ): Observable<HealthCheckHistory[]> {

    return this.http.get<HealthCheckHistory[]>(
      `${this.baseUrl}/health/${encodeURIComponent(serviceName)}/instances/${encodeURIComponent(instanceId)}/history`
    );
  }

  getInstanceDowntime(
    serviceName: string,
    instanceId: string
  ): Observable<InstanceDowntime> {

    return this.http.get<InstanceDowntime>(
      `${this.baseUrl}/health/${encodeURIComponent(serviceName)}/instances/${encodeURIComponent(instanceId)}/downtime`
    );
  }

}