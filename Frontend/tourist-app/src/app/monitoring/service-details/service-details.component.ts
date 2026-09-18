import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import {
  ChartConfiguration,
  ChartData
} from 'chart.js';

import {
  DowntimePeriod,
  HealthCheckHistory,
  InstanceHealthStatus,
  MonitoringService,
  ResponseTimeStats,
  ServiceHealthStatus,
  UptimeStats
} from '../../services/monitoring.service';

@Component({
  selector: 'app-service-details',
  templateUrl: './service-details.component.html',
  styleUrls: ['./service-details.component.css']
})
export class ServiceDetailsComponent implements OnInit {

  serviceName = '';

  serviceHealth: ServiceHealthStatus | null = null;

  instances: InstanceHealthStatus[] = [];

  healthHistory: HealthCheckHistory[] = [];

  downtimePeriods: DowntimePeriod[] = [];

  responseTimeStats: ResponseTimeStats | null = null;

  uptimeStats: UptimeStats | null = null;

  isLoading = true;

  errorMessage = '';

  selectedInstance: InstanceHealthStatus | null = null;

  instanceHistory: HealthCheckHistory[] = [];

  instanceDowntimePeriods: DowntimePeriod[] = [];

  isLoadingInstanceDetails = false;

  instanceDetailsError = '';

  responseTimeChartType: 'line' = 'line';

  responseTimeChartData: ChartData<'line'> = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Vreme odziva (ms)',
        tension: 0.2,
        borderColor: '#4a7df0',
        backgroundColor: 'rgba(74, 125, 240, 0.12)',
        pointBackgroundColor: '#4a7df0',
        pointBorderColor: '#4a7df0',
        fill: true
      }
    ]
  };

  responseTimeChartOptions:
    ChartConfiguration<'line'>['options'] = {

    responsive: true,

    maintainAspectRatio: false,

    plugins: {
      legend: {
        display: true
      }
    },

    scales: {

      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: 'Vreme odziva (ms)'
        }
      },

      x: {
        title: {
          display: true,
          text: 'Vreme'
        },
        ticks: {
          maxRotation: 0,
          minRotation: 0,
          autoSkip: true,
          maxTicksLimit: 12
        }
      }

    }

  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private monitoringService: MonitoringService
  ) {}

  ngOnInit(): void {

    this.serviceName =
      this.route.snapshot.paramMap
        .get('serviceName') ?? '';

    if (!this.serviceName) {

      this.errorMessage =
        'Naziv servisa nije prosleđen.';

      this.isLoading = false;

      return;
    }

    this.loadServiceDetails();
  }

  loadServiceDetails(): void {

    this.isLoading = true;

    this.errorMessage = '';

    forkJoin({

      health:
        this.monitoringService
          .getServiceHealth(
            this.serviceName
          ),

      instances:
        this.monitoringService
          .getServiceInstances(
            this.serviceName
          ),

      history:
        this.monitoringService
          .getServiceHistory(
            this.serviceName
          ),

      downtime:
        this.monitoringService
          .getServiceDowntime(
            this.serviceName
          ),

      responseTime:
        this.monitoringService
          .getServiceResponseTime(
            this.serviceName
          ),

      uptime:
        this.monitoringService
          .getServiceUptime(
            this.serviceName
          )

    }).subscribe({

      next: (result) => {

        this.serviceHealth =
          result.health;

        this.instances =
          result.instances;

        this.healthHistory =
          result.history;

        this.downtimePeriods =
          result.downtime
            .downtimePeriods;

        this.responseTimeStats =
          result.responseTime;

        this.uptimeStats =
          result.uptime;

        this.buildResponseTimeChart();

        this.isLoading = false;
      },

      error: (error) => {

        console.error(
          'Failed to load service details.',
          error
        );

        this.errorMessage =
          'Nije moguće učitati detalje servisa.';

        this.isLoading = false;
      }

    });
  }

  openInstanceDetails(
    instance: InstanceHealthStatus
  ): void {

    this.selectedInstance =
      instance;

    this.instanceHistory = [];

    this.instanceDowntimePeriods = [];

    this.instanceDetailsError = '';

    this.isLoadingInstanceDetails = true;

    forkJoin({

      history:
        this.monitoringService
          .getInstanceHistory(
            this.serviceName,
            instance.instanceId
          ),

      downtime:
        this.monitoringService
          .getInstanceDowntime(
            this.serviceName,
            instance.instanceId
          )

    }).subscribe({

      next: (result) => {

        this.instanceHistory =
          result.history;

        this.instanceDowntimePeriods =
          result.downtime
            .downtimePeriods;

        this.isLoadingInstanceDetails =
          false;
      },

      error: (error) => {

        console.error(
          'Failed to load instance details.',
          error
        );

        this.instanceDetailsError =
          'Nije moguće učitati detalje instance.';

        this.isLoadingInstanceDetails =
          false;
      }

    });
  }

  closeInstanceDetails(): void {

    this.selectedInstance = null;

    this.instanceHistory = [];

    this.instanceDowntimePeriods = [];

    this.instanceDetailsError = '';

    this.isLoadingInstanceDetails = false;
  }

  viewServiceLogs(): void {

    this.router.navigate(
      ['/monitoring/logs'],
      {
        queryParams: {
          service: this.serviceName
        }
      }
    );
  }

  private buildResponseTimeChart(): void {

    const orderedHistory =
      [...this.healthHistory]
        .filter(history =>
          history.status === 'UP' &&
          history.responseTimeMs !== null
        )
        .sort(
          (a, b) =>
            new Date(a.checkedAt).getTime() -
            new Date(b.checkedAt).getTime()
        );

    this.responseTimeChartData = {

      labels:
        orderedHistory.map(history =>
          new Date(history.checkedAt)
            .toLocaleTimeString(
              'sr-RS',
              {
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit'
              }
            )
        ),

      datasets: [
        {
          data:
            orderedHistory.map(
              history =>
                history.responseTimeMs!
            ),

          label:
            'Vreme odziva (ms)',

          tension: 0.2,

          borderColor:
            '#4a7df0',

          backgroundColor:
            'rgba(74, 125, 240, 0.12)',

          pointBackgroundColor:
            '#4a7df0',

          pointBorderColor:
            '#4a7df0',

          fill:
            true
        }
      ]

    };
  }

}