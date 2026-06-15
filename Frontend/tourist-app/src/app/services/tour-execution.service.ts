import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TourExecution {
  id: number;
  tourId: number;
  touristUsername: string;
  status: string;
  startedAt?: string;
  completedAt?: string;
  abandonedAt?: string;
  lastActivityAt?: string;
  completedKeyPoints?: any[];
}

export interface TouristLocation {
  id?: number;
  touristUsername?: string;
  latitude: number;
  longitude: number;
}

export interface CheckLocationResponse {
  nearKeyPoint?: boolean;
  keyPointCompleted?: boolean;
  message?: string;
  keyPointName?: string;
  completedKeyPoint?: any;
}

@Injectable({
  providedIn: 'root'
})
export class TourExecutionService {
  private executionApiUrl = '/execution-api';
  private locationApiUrl = '/location-api';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('userToken');

    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  startTour(tourId: number): Observable<TourExecution> {
    return this.http.post<TourExecution>(
      `${this.executionApiUrl}/${tourId}/start`,
      {},
      { headers: this.getHeaders() }
    );
  }

  getActiveExecution(): Observable<TourExecution> {
    return this.http.get<TourExecution>(
      `${this.executionApiUrl}/active`,
      { headers: this.getHeaders() }
    );
  }

  checkLocation(executionId: number): Observable<CheckLocationResponse> {
    return this.http.post<CheckLocationResponse>(
      `${this.executionApiUrl}/${executionId}/check-location`,
      {},
      { headers: this.getHeaders() }
    );
  }

  completeTour(executionId: number): Observable<TourExecution> {
    return this.http.post<TourExecution>(
      `${this.executionApiUrl}/${executionId}/complete`,
      {},
      { headers: this.getHeaders() }
    );
  }

  abandonTour(executionId: number): Observable<TourExecution> {
    return this.http.post<TourExecution>(
      `${this.executionApiUrl}/${executionId}/abandon`,
      {},
      { headers: this.getHeaders() }
    );
  }

  setLocation(latitude: number, longitude: number): Observable<TouristLocation> {
    return this.http.post<TouristLocation>(
      this.locationApiUrl,
      { latitude, longitude },
      { headers: this.getHeaders() }
    );
  }

  getLocation(): Observable<TouristLocation> {
    return this.http.get<TouristLocation>(
      this.locationApiUrl,
      { headers: this.getHeaders() }
    );
  }
}