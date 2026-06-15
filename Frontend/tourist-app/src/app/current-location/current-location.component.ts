import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { AuthService } from '../services/auth.service';
import { LocationService, TouristLocation } from '../services/location.service';

@Component({
  selector: 'app-current-location',
  templateUrl: './current-location.component.html',
  styleUrls: ['./current-location.component.css']
})
export class CurrentLocationComponent implements OnInit, AfterViewInit, OnDestroy {
  currentUser: any = null;
  location: TouristLocation | null = null;
  selectedLatitude: number | null = null;
  selectedLongitude: number | null = null;
  isSaving = false;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  private map: L.Map | null = null;
  private marker: L.CircleMarker | null = null;
  private readonly defaultCenter: L.LatLngExpression = [44.7866, 20.4489];

  constructor(
    private authService: AuthService,
    private locationService: LocationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getUserDetails();
    if (!this.currentUser) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.currentUser.role !== 'Tourist') {
      this.errorMessage = 'Samo turista moze da koristi simulator trenutne lokacije.';
      return;
    }

    this.loadCurrentLocation();
  }

  ngAfterViewInit(): void {
    this.initializeMap();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  saveLocation(): void {
    if (this.selectedLatitude === null || this.selectedLongitude === null) {
      this.errorMessage = 'Klikni na mapu da izaberes trenutnu lokaciju.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.locationService.setCurrentLocation(this.selectedLatitude, this.selectedLongitude).subscribe({
      next: location => {
        this.location = location;
        this.selectedLatitude = location.latitude;
        this.selectedLongitude = location.longitude;
        this.placeMarker(location.latitude, location.longitude);
        this.successMessage = 'Trenutna lokacija je sacuvana.';
        this.isSaving = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce sacuvati trenutnu lokaciju.';
        this.isSaving = false;
      }
    });
  }

  private loadCurrentLocation(): void {
    this.isLoading = true;

    this.locationService.getCurrentLocation().subscribe({
      next: location => {
        this.location = location;
        if (location) {
          this.selectedLatitude = location.latitude;
          this.selectedLongitude = location.longitude;
          this.placeMarker(location.latitude, location.longitude);
          this.map?.setView([location.latitude, location.longitude], 13);
        }
        this.isLoading = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce ucitati trenutnu lokaciju.';
        this.isLoading = false;
      }
    });
  }

  private initializeMap(): void {
    this.map = L.map('currentLocationMap').setView(this.defaultCenter, 12);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    this.map.on('click', (event: L.LeafletMouseEvent) => {
      this.selectedLatitude = event.latlng.lat;
      this.selectedLongitude = event.latlng.lng;
      this.placeMarker(event.latlng.lat, event.latlng.lng);
      this.successMessage = '';
      this.errorMessage = '';
    });

    setTimeout(() => this.map?.invalidateSize(), 0);
  }

  private placeMarker(latitude: number, longitude: number): void {
    if (!this.map) return;

    if (this.marker) {
      this.marker.setLatLng([latitude, longitude]);
      return;
    }

    this.marker = L.circleMarker([latitude, longitude], {
      radius: 9,
      color: '#1d4ed8',
      weight: 3,
      fillColor: '#3b82f6',
      fillOpacity: 0.9
    }).addTo(this.map);
  }
}
