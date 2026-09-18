import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { Router } from '@angular/router';

import {
  Alert,
  MonitoringService
} from '../../services/monitoring.service';

@Component({
  selector: 'app-alerts',
  templateUrl: './alerts.component.html',
  styleUrls: ['./alerts.component.css']
})
export class AlertsComponent implements OnInit {

  activeAlerts: Alert[] = [];
  alertHistory: Alert[] = [];
  selectedAlert: Alert | null = null;

  isLoading = true;
  errorMessage = '';

  readonly alertHistoryPageSize = 20;
  visibleAlertHistoryCount = 20;

  constructor(
    private monitoringService: MonitoringService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAlerts();
  }

  loadAlerts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      allAlerts: this.monitoringService.getAlerts(),
      activeAlerts: this.monitoringService.getActiveAlerts()
    }).subscribe({
      next: (result) => {
        this.activeAlerts = result.activeAlerts;

        this.alertHistory = result.allAlerts.filter(
          alert => alert.status !== 'ACTIVE'
        );

        this.visibleAlertHistoryCount =
          this.alertHistoryPageSize;

        this.isLoading = false;
      },
      error: (error) => {
        console.error(
          'Failed to load alerts.',
          error
        );

        this.errorMessage =
          'Nije moguće učitati alerte.';

        this.isLoading = false;
      }
    });
  }

  get visibleAlertHistory(): Alert[] {
    return this.alertHistory.slice(
      0,
      this.visibleAlertHistoryCount
    );
  }

  get hasMoreAlertHistory(): boolean {
    return (
      this.visibleAlertHistoryCount <
      this.alertHistory.length
    );
  }

  onAlertHistoryScroll(event: Event): void {
    const element = event.target as HTMLElement;

    const distanceFromBottom =
      element.scrollHeight -
      element.scrollTop -
      element.clientHeight;

    if (
      distanceFromBottom < 80 &&
      this.hasMoreAlertHistory
    ) {
      this.visibleAlertHistoryCount =
        Math.min(
          this.visibleAlertHistoryCount +
            this.alertHistoryPageSize,
          this.alertHistory.length
        );
    }
  }

  viewRelatedLogs(alert: Alert): void {
    const triggeredAt = new Date(alert.triggeredAt);

    const from = new Date(
      triggeredAt.getTime() - 60 * 1000
    ).toISOString();

    const queryParams: any = {
      service: alert.serviceName,
      from: from
    };

    if (alert.resolvedAt) {
      const resolvedAt = new Date(alert.resolvedAt);

      queryParams.to = new Date(
        resolvedAt.getTime() + 60 * 1000
      ).toISOString();
    }

    this.router.navigate(
      ['/monitoring/logs'],
      { queryParams }
    );
  }

  selectAlert(alert: Alert): void {
    this.selectedAlert = alert;
  }

  closeAlertDetails(): void {
    this.selectedAlert = null;
  }

  getAlertDuration(alert: Alert): string {
    const start = new Date(alert.triggeredAt).getTime();

    const end = alert.resolvedAt
      ? new Date(alert.resolvedAt).getTime()
      : Date.now();

    const totalSeconds = Math.max(
      0,
      Math.floor((end - start) / 1000)
    );

    const hours = Math.floor(
      totalSeconds / 3600
    );

    const minutes = Math.floor(
      (totalSeconds % 3600) / 60
    );

    const seconds = totalSeconds % 60;

    if (hours > 0) {
      return `${hours}h ${minutes}m ${seconds}s`;
    }

    if (minutes > 0) {
      return `${minutes}m ${seconds}s`;
    }

    return `${seconds}s`;
  }

}