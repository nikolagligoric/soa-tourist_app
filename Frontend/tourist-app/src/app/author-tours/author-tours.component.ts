import { AfterViewChecked, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { AuthService } from '../services/auth.service';
import {
  CreateTourRequest,
  KeyPoint,
  TourDuration,
  TourDetails,
  TourService
} from '../services/tour.service';

@Component({
  selector: 'app-author-tours',
  templateUrl: './author-tours.component.html',
  styleUrls: ['./author-tours.component.css']
})
export class AuthorToursComponent implements OnInit, AfterViewChecked, OnDestroy {
  tours: TourDetails[] = [];
  keyPoints: KeyPoint[] = [];
  selectedTour: TourDetails | null = null;
  currentUser: any = null;
  difficulties = ['EASY', 'MEDIUM', 'HARD'];
  transportTypes = [
    { value: 'WALKING', label: 'Peske' },
    { value: 'BICYCLE', label: 'Bicikl' },
    { value: 'CAR', label: 'Automobil' }
  ];

  tourForm: CreateTourRequest = {
    name: '',
    description: '',
    difficulty: 'EASY',
    tags: ''
  };

  keyPointForm = {
    name: '',
    description: '',
    imageUrl: ''
  };

  durationForm = {
    transportType: 'WALKING',
    durationMinutes: 60
  };

  selectedLatitude: number | null = null;
  selectedLongitude: number | null = null;
  isLoading = false;
  isSavingTour = false;
  isSavingKeyPoint = false;
  isSavingDuration = false;
  isPublishing = false;
  isChangingStatus = false;
  editingKeyPoint: KeyPoint | null = null;
  successMessage = '';
  errorMessage = '';

  private map: L.Map | null = null;
  private selectedMarker: L.CircleMarker | null = null;
  private keyPointMarkers: L.Marker[] = [];
  private routeLine: L.Polyline | null = null;
  private routeRequestId = 0;
  private shouldInitMap = false;
  private readonly defaultCenter: L.LatLngExpression = [44.7866, 20.4489];

  constructor(
    private authService: AuthService,
    private tourService: TourService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getUserDetails();

    if (!this.currentUser) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.currentUser.role !== 'Guide') {
      this.errorMessage = 'Samo autor moze da upravlja svojim turama.';
      return;
    }

    this.loadTours();
  }

  ngAfterViewChecked(): void {
    if (!this.shouldInitMap) return;

    this.shouldInitMap = false;
    this.initializeMap();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  createTour(): void {
    this.isSavingTour = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.createTour(this.tourForm).subscribe({
      next: tour => {
        this.tours = [tour, ...this.tours];
        this.tourForm = { name: '', description: '', difficulty: 'EASY', tags: '' };
        this.successMessage = 'Tura je kreirana kao draft sa cenom 0.';
        this.isSavingTour = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce kreirati turu.';
        this.isSavingTour = false;
      }
    });
  }

  selectTour(tour: TourDetails): void {
    this.selectedTour = tour;
    this.keyPoints = [];
    this.successMessage = '';
    this.errorMessage = '';
    this.resetKeyPointForm();
    this.map?.remove();
    this.map = null;
    this.keyPointMarkers = [];
    this.routeLine = null;
    this.shouldInitMap = true;
    this.loadKeyPoints(tour.id);
  }

  saveKeyPoint(): void {
    if (!this.selectedTour) return;

    if (!this.keyPointForm.name.trim() || !this.keyPointForm.description.trim() || !this.keyPointForm.imageUrl.trim()) {
      this.errorMessage = 'Unesi naziv, opis i sliku za kljucnu tacku.';
      return;
    }

    if (this.selectedLatitude === null || this.selectedLongitude === null) {
      this.errorMessage = 'Klikni na mapu da izaberes lokaciju kljucne tacke.';
      return;
    }

    this.isSavingKeyPoint = true;
    this.successMessage = '';
    this.errorMessage = '';

    const request = {
      name: this.keyPointForm.name,
      description: this.keyPointForm.description,
      imageUrl: this.keyPointForm.imageUrl,
      latitude: this.selectedLatitude,
      longitude: this.selectedLongitude
    };

    const wasEditing = !!this.editingKeyPoint;
    const operation = this.editingKeyPoint
      ? this.tourService.updateKeyPoint(this.selectedTour.id, this.editingKeyPoint.id, request)
      : this.tourService.addKeyPoint(this.selectedTour.id, request);

    operation.subscribe({
      next: keyPoint => {
        this.keyPoints = wasEditing
          ? this.keyPoints.map(existing => existing.id === keyPoint.id ? keyPoint : existing)
          : [...this.keyPoints, keyPoint];
        this.keyPoints = this.keyPoints.sort((a, b) => a.sequence - b.sequence);
        this.renderKeyPointMarkers();
        this.resetKeyPointForm();
        this.successMessage = wasEditing ? 'Kljucna tacka je izmenjena.' : 'Kljucna tacka je dodata turi.';
        this.isSavingKeyPoint = false;
        this.loadTours(false);
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce dodati kljucnu tacku.';
        this.isSavingKeyPoint = false;
      }
    });
  }

  startEditKeyPoint(keyPoint: KeyPoint): void {
    this.editingKeyPoint = keyPoint;
    this.keyPointForm = {
      name: keyPoint.name,
      description: keyPoint.description,
      imageUrl: keyPoint.imageUrl
    };
    this.selectedLatitude = null;
    this.selectedLongitude = null;
    this.placeSelectedMarker(keyPoint.latitude, keyPoint.longitude);
    this.map?.setView([keyPoint.latitude, keyPoint.longitude], 15);
    this.successMessage = '';
    this.errorMessage = 'Klikni na mapu da izaberes novu poziciju za izmenu tacke.';
  }

  cancelEditKeyPoint(): void {
    this.resetKeyPointForm();
    this.errorMessage = '';
  }

  deleteKeyPoint(keyPoint: KeyPoint): void {
    if (!this.selectedTour) return;

    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.deleteKeyPoint(this.selectedTour.id, keyPoint.id).subscribe({
      next: () => {
        this.keyPoints = this.keyPoints.filter(existing => existing.id !== keyPoint.id);
        this.renderKeyPointMarkers();
        this.resetKeyPointForm();
        this.successMessage = 'Kljucna tacka je obrisana.';
        this.loadTours(false);
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce obrisati kljucnu tacku.';
      }
    });
  }

  addDuration(): void {
    if (!this.selectedTour) return;

    if (!this.durationForm.durationMinutes || this.durationForm.durationMinutes <= 0) {
      this.errorMessage = 'Vreme obilaska mora biti vece od 0 minuta.';
      return;
    }

    this.isSavingDuration = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.addDuration(this.selectedTour.id, this.durationForm).subscribe({
      next: duration => {
        const durations = [...this.getDurations(), duration];
        this.selectedTour = { ...this.selectedTour!, durations };
        this.updateSelectedTourInList(this.selectedTour);
        this.durationForm = { transportType: 'WALKING', durationMinutes: 60 };
        this.successMessage = 'Vreme obilaska je dodato.';
        this.isSavingDuration = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce dodati vreme obilaska.';
        this.isSavingDuration = false;
      }
    });
  }

  publishSelectedTour(): void {
    if (!this.selectedTour) return;

    const validationError = this.getPublishValidationError();
    if (validationError) {
      this.errorMessage = validationError;
      return;
    }

    this.isPublishing = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.publishTour(this.selectedTour.id).subscribe({
      next: tour => {
        this.selectedTour = { ...this.selectedTour!, ...tour };
        this.updateSelectedTourInList(this.selectedTour);
        this.successMessage = 'Tura je poslata na objavljivanje.';
        this.isPublishing = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce objaviti turu.';
        this.isPublishing = false;
      }
    });
  }

  archiveSelectedTour(): void {
    if (!this.selectedTour) return;

    this.isChangingStatus = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.archiveTour(this.selectedTour.id).subscribe({
      next: tour => {
        this.selectedTour = { ...this.selectedTour!, ...tour };
        this.updateSelectedTourInList(this.selectedTour);
        this.successMessage = 'Tura je arhivirana.';
        this.isChangingStatus = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce arhivirati turu.';
        this.isChangingStatus = false;
      }
    });
  }

  reactivateSelectedTour(): void {
    if (!this.selectedTour) return;

    this.isChangingStatus = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.reactivateTour(this.selectedTour.id).subscribe({
      next: tour => {
        this.selectedTour = { ...this.selectedTour!, ...tour };
        this.updateSelectedTourInList(this.selectedTour);
        this.successMessage = 'Tura je ponovo aktivirana.';
        this.isChangingStatus = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce ponovo aktivirati turu.';
        this.isChangingStatus = false;
      }
    });
  }

  getDurations(): TourDuration[] {
    return (this.selectedTour?.durations || []) as TourDuration[];
  }

  getPublishValidationError(): string {
    if (!this.selectedTour) return 'Izaberi turu.';
    if (!this.selectedTour.name || !this.selectedTour.description || !this.selectedTour.difficulty || !this.selectedTour.tags) {
      return 'Tura mora imati naziv, opis, tezinu i tagove.';
    }
    if (this.keyPoints.length < 2) {
      return 'Tura mora imati bar dve kljucne tacke.';
    }
    if (this.getDurations().length < 1) {
      return 'Dodaj bar jedno vreme obilaska pre objavljivanja.';
    }
    return '';
  }

  private loadTours(showLoader = true): void {
    this.isLoading = showLoader;

    this.tourService.getToursByAuthor(this.currentUser.username).subscribe({
      next: tours => {
        this.tours = tours || [];
        if (this.selectedTour) {
          const refreshedTour = this.tours.find(tour => tour.id === this.selectedTour?.id);
          if (refreshedTour) {
            this.selectedTour = refreshedTour;
          }
        }
        this.isLoading = false;
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce ucitati ture autora.';
        this.isLoading = false;
      }
    });
  }

  private loadKeyPoints(tourId: number): void {
    this.tourService.getKeyPoints(tourId).subscribe({
      next: keyPoints => {
        this.keyPoints = keyPoints || [];
        this.renderKeyPointMarkers();
      },
      error: err => {
        this.errorMessage = err.error?.message || err.error || 'Nije moguce ucitati kljucne tacke.';
      }
    });
  }

  private initializeMap(): void {
    if (!this.selectedTour || this.map) return;

    this.map = L.map('authorTourMap').setView(this.defaultCenter, 12);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    this.map.on('click', (event: L.LeafletMouseEvent) => {
      this.selectedLatitude = event.latlng.lat;
      this.selectedLongitude = event.latlng.lng;
      this.placeSelectedMarker(event.latlng.lat, event.latlng.lng);
      this.errorMessage = '';
    });

    this.renderKeyPointMarkers();
    setTimeout(() => this.map?.invalidateSize(), 0);
  }

  private placeSelectedMarker(latitude: number, longitude: number): void {
    if (!this.map) return;

    if (this.selectedMarker) {
      this.selectedMarker.setLatLng([latitude, longitude]);
      return;
    }

    this.selectedMarker = L.circleMarker([latitude, longitude], {
      radius: 9,
      color: '#047857',
      weight: 3,
      fillColor: '#10b981',
      fillOpacity: 0.9
    }).addTo(this.map);
  }

  private renderKeyPointMarkers(): void {
    if (!this.map) return;

    this.keyPointMarkers.forEach(marker => marker.remove());
    this.keyPointMarkers = [];
    this.keyPoints.forEach(keyPoint => this.addKeyPointMarker(keyPoint));
    this.drawRouteLine();

    if (this.keyPoints.length > 0) {
      const bounds = L.latLngBounds(this.keyPoints.map(point => [point.latitude, point.longitude]));
      this.map.fitBounds(bounds, { padding: [28, 28], maxZoom: 15 });
    }
  }

  private addKeyPointMarker(keyPoint: KeyPoint): void {
    if (!this.map) return;

    const marker = L.marker([keyPoint.latitude, keyPoint.longitude])
      .bindPopup(`${keyPoint.sequence}. ${keyPoint.name}`)
      .on('click', () => this.startEditKeyPoint(keyPoint))
      .addTo(this.map);
    this.keyPointMarkers.push(marker);
  }

  private async drawRouteLine(): Promise<void> {
    if (!this.map) return;

    const requestId = ++this.routeRequestId;
    this.routeLine?.remove();
    this.routeLine = null;

    if (this.keyPoints.length < 2) return;

    const fallbackCoordinates = this.keyPoints.map(point => [point.latitude, point.longitude] as L.LatLngExpression);

    try {
      const coordinates = this.keyPoints
        .map(point => `${point.longitude},${point.latitude}`)
        .join(';');
      const response = await fetch(
        `https://router.project-osrm.org/route/v1/driving/${coordinates}?overview=full&geometries=geojson`
      );
      const data = await response.json();
      const routeCoordinates = data?.routes?.[0]?.geometry?.coordinates;

      if (requestId !== this.routeRequestId || !this.map) return;

      if (!Array.isArray(routeCoordinates) || routeCoordinates.length === 0) {
        this.routeLine = L.polyline(fallbackCoordinates, { color: '#047857', weight: 4, opacity: 0.85 }).addTo(this.map);
        return;
      }

      this.routeLine = L.polyline(
        routeCoordinates.map(([longitude, latitude]: [number, number]) => [latitude, longitude]),
        { color: '#047857', weight: 4, opacity: 0.85 }
      ).addTo(this.map);
    } catch {
      if (requestId !== this.routeRequestId || !this.map) return;
      this.routeLine = L.polyline(fallbackCoordinates, { color: '#047857', weight: 4, opacity: 0.85 }).addTo(this.map);
    }
  }

  private resetKeyPointForm(): void {
    this.keyPointForm = { name: '', description: '', imageUrl: '' };
    this.editingKeyPoint = null;
    this.selectedLatitude = null;
    this.selectedLongitude = null;
    this.selectedMarker?.remove();
    this.selectedMarker = null;
  }

  private updateSelectedTourInList(tour: TourDetails): void {
    this.tours = this.tours.map(existing => existing.id === tour.id ? tour : existing);
  }
}
