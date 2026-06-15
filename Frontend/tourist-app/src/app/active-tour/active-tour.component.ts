import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  CheckLocationResponse,
  TourExecution,
  TourExecutionService,
  TouristLocation
} from '../services/tour-execution.service';

@Component({
  selector: 'app-active-tour',
  templateUrl: './active-tour.component.html',
  styleUrls: ['./active-tour.component.css']
})
export class ActiveTourComponent implements OnInit {
  tourId: number | null = null;
  execution: TourExecution | null = null;
  location: TouristLocation | null = null;

  latitude: number | null = null;
  longitude: number | null = null;

  isLoading = false;
  errorMessage = '';
  successMessage = '';
  locationMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private executionService: TourExecutionService
  ) {}

  ngOnInit(): void {
    const queryTourId = this.route.snapshot.queryParamMap.get('tourId');

    if (queryTourId) {
      this.tourId = Number(queryTourId);
      this.startTour();
    } else { 
      this.loadActiveExecution();
    }
  }

  startTour(): void {
    if (!this.tourId) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.executionService.startTour(this.tourId).subscribe({
      next: (data) => {
        this.execution = data;
        this.successMessage = 'Tura uspešno započeta.';
        this.isLoading = false;
        this.loadLocation();
      },
      error: (error) => {
        const message =
          typeof error?.error === 'string'
            ? error.error
            : error?.error?.message || '';

        if (
          message.toLowerCase().includes('already have an active execution') ||
          message.toLowerCase().includes('active execution')
        ) {
          this.errorMessage = '';
          this.successMessage = '';
          this.isLoading = false;

          this.loadActiveExecution();
          return;
        }

        this.errorMessage = message || 'Greška pri započinjanju ture.';
        this.isLoading = false;
      }
    });
  }

  loadActiveExecution(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.executionService.getActiveExecution().subscribe({
      next: (data) => {
        this.execution = data;
        this.isLoading = false;
        this.loadLocation();
      },
      error: () => {
        this.errorMessage = 'Ne postoji ni jedna aktivna tura.';
        this.isLoading = false;
      }
    });
  }

  loadLocation(): void {
    this.executionService.getLocation().subscribe({
      next: (data) => {
        this.location = data;
        this.latitude = data.latitude;
        this.longitude = data.longitude;
      },
      error: () => {
        this.locationMessage = 'Trenutna lokacija nije podešena.';
      }
    });
  }

  saveLocation(): void {
    if (this.latitude === null || this.longitude === null) {
      this.locationMessage = 'Unesite geografsku širinu i visinu.';
      return;
    }

    this.locationMessage = '';

    this.executionService.setLocation(this.latitude, this.longitude).subscribe({
      next: (data) => {
        this.location = data;
        this.locationMessage = 'Lokacija uspešno sačuvana,';
      },
      error: () => {
        this.locationMessage = 'Greška pri čuvanju lokacije';
      }
    });
  }

  checkLocation(): void {
    if (!this.execution) return;

    this.successMessage = '';
    this.errorMessage = '';

    this.executionService.checkLocation(this.execution.id).subscribe({
      next: (response: CheckLocationResponse) => {
        this.successMessage =
          response.message ||
          response.keyPointName ||
          'Provera lokacije uspešna.';
        this.loadActiveExecution();
      },
      error: () => {
        this.errorMessage = 'Greška pri proveri lokacije';
      }
    });
  }

  completeTour(): void {
    if (!this.execution) return;

    this.executionService.completeTour(this.execution.id).subscribe({
      next: (data) => {
        this.execution = data;
        this.successMessage = 'Tura uspešno završena.';
      },
      error: () => {
        this.errorMessage = 'Greška pri završavanju ture.';
      }
    });
  }

  abandonTour(): void {
    if (!this.execution) return;

    this.executionService.abandonTour(this.execution.id).subscribe({
      next: (data) => {
        this.execution = data;
        this.successMessage = 'Tura napuštena.';
      },
      error: () => {
        this.errorMessage = 'Greška pri napuštanju ture.';
      }
    });
  }

  goToPurchases(): void {
    this.router.navigate(['/purchases']);
  }
}