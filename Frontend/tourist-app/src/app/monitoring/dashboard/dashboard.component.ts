import { Component, OnInit } from '@angular/core';
import {
  HealthSummary,
  LogSummary,
  LogsByService,
  LogTrend,
  ResponseTimeByService,
  AvailabilityByService,
  MonitoringService
} from '../../services/monitoring.service';

import {
  ChartConfiguration,
  ChartData
} from 'chart.js';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  healthSummary: HealthSummary | null = null;
  logSummary: LogSummary | null = null;
  logsByService: LogsByService | null = null;
  logTrend: LogTrend | null = null;
  responseTimeByService: ResponseTimeByService | null = null;
  availabilityByService: AvailabilityByService | null = null;

  isLoading = true;
  errorMessage = '';

  selectedHours = 24;

  periodOptions = [
    {
      label: 'Poslednjih 1 sat',
      hours: 1
    },
    {
      label: 'Poslednjih 6 sati',
      hours: 6
    },
    {
      label: 'Poslednja 24 sata',
      hours: 24
    },
    {
      label: 'Poslednjih 7 dana',
      hours: 168
    }
  ];

  constructor(
    private monitoringService: MonitoringService
  ) {}

  ngOnInit(): void {
    this.loadHealthSummary();
    this.loadLogSummary();
  }

  lineChartData: ChartData<'line'> = {
    labels: [],
    datasets: [
      {
        label: 'Ukupno logova',
        data: []
      },
      {
        label: 'Greške',
        data: []
      }
    ]
  };

  lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  barChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Greške',
        data: []
      },
      {
        label: 'Upozorenja',
        data: []
      }
    ]
  };

  barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true
      }
    },
    scales: {
      x: {
        ticks: {
          maxRotation: 0,
          minRotation: 0,
          autoSkip: false
        }
      },
      y: {
        beginAtZero: true
      }
    }
  };

  responseTimeChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Prosečno vreme odziva (ms)',
        data: []
      }
    ]
  };

  responseTimeChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  availabilityChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Dostupnost (%)',
        data: []
      }
    ]
  };

  availabilityChartOptions: ChartConfiguration<'bar'>['options'] = {
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
        max: 100
      }
    }
  };

  onPeriodChange(): void {
    this.loadLogSummary();
  }

  loadHealthSummary(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.monitoringService
      .getHealthSummary()
      .subscribe({
        next: (summary) => {
          this.healthSummary = summary;
          this.isLoading = false;
        },
        error: (error) => {
          console.error(
            'Failed to load monitoring health summary.',
            error
          );

          this.errorMessage =
            'Nije moguće učitati monitoring podatke.';

          this.isLoading = false;
        }
      });
  }

  loadLogSummary(): void {
    this.monitoringService
      .getLogSummary(
        this.selectedHours
      )
      .subscribe({
        next: (summary) => {
          this.logSummary = summary;
          this.loadLogsByService();
        },
        error: (error) => {
          console.error(
            'Failed to load log summary.',
            error
          );
        }
      });
  }

  loadLogsByService(): void {
    this.monitoringService
      .getLogsByService(
        this.selectedHours
      )
      .subscribe({
        next: (data) => {
          this.logsByService = data;

          this.barChartData = {
            labels: data.services.map(
              service => service.serviceName
            ),
            datasets: [
              {
                label: 'Greške',
                data: data.services.map(
                  service => service.errorLogs
                ),
                backgroundColor: '#ef5350',
                borderColor: '#ef5350'
              },
              {
                label: 'Upozorenja',
                data: data.services.map(
                  service => service.warningLogs
                ),
                backgroundColor: '#f4b740',
                borderColor: '#f4b740'
              }
            ]
          };

          this.loadLogTrend();
        },
        error: (error) => {
          console.error(
            'Failed to load logs by service.',
            error
          );
        }
      });
  }

  loadLogTrend(): void {
    this.monitoringService
      .getLogTrend(
        this.selectedHours
      )
      .subscribe({
        next: (data) => {
          this.logTrend = data;

          this.lineChartData = {
            labels: data.points.map(point => {
              const date = new Date(point.timestamp);

              if (this.selectedHours > 24) {
                return date.toLocaleString(
                  'sr-RS',
                  {
                    day: '2-digit',
                    month: '2-digit',
                    hour: '2-digit',
                    minute: '2-digit'
                  }
                );
              }

              return date.toLocaleTimeString(
                'sr-RS',
                {
                  hour: '2-digit',
                  minute: '2-digit'
                }
              );
            }),
            datasets: [
              {
                label: 'Ukupno logova',
                data: data.points.map(
                  point => point.totalLogs
                ),
                borderColor: '#4a7df0',
                backgroundColor: '#4a7df0',
                pointBackgroundColor: '#4a7df0',
                pointBorderColor: '#4a7df0'
              },
              {
                label: 'Greške',
                data: data.points.map(
                  point => point.errorLogs
                ),
                borderColor: '#ef5350',
                backgroundColor: '#ef5350',
                pointBackgroundColor: '#ef5350',
                pointBorderColor: '#ef5350'
              }
            ]
          };

          this.loadResponseTimeByService();
        },
        error: (error) => {
          console.error(
            'Failed to load log trend.',
            error
          );
        }
      });
  }

  loadResponseTimeByService(): void {
    this.monitoringService
      .getResponseTimeByService(
        this.selectedHours
      )
      .subscribe({
        next: (data) => {
          this.responseTimeByService = data;

          this.responseTimeChartData = {
            labels: data.services.map(
              service => service.serviceName
            ),
            datasets: [
              {
                label: 'Prosečno vreme odziva (ms)',
                data: data.services.map(
                  service =>
                    service.averageResponseTimeMs ?? 0
                ),
                backgroundColor: '#4a7df0',
                borderColor: '#4a7df0'
              }
            ]
          };

          this.loadAvailabilityByService();
        },
        error: (error) => {
          console.error(
            'Failed to load response time by service.',
            error
          );
        }
      });
  }

  loadAvailabilityByService(): void {
    this.monitoringService
      .getAvailabilityByService(
        this.selectedHours
      )
      .subscribe({
        next: (data) => {
          this.availabilityByService = data;

          this.availabilityChartData = {
            labels: data.services.map(
              service => service.serviceName
            ),
            datasets: [
              {
                label: 'Dostupnost (%)',
                data: data.services.map(
                  service =>
                    service.availabilityPercentage ?? 0
                ),
                backgroundColor: '#58b77a',
                borderColor: '#58b77a'
              }
            ]
          };
        },
        error: (error) => {
          console.error(
            'Failed to load availability by service.',
            error
          );
        }
      });
  }
}