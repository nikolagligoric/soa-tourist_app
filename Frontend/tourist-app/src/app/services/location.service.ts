import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TouristLocation {
  id?: number;
  username?: string;
  latitude: number;
  longitude: number;
}

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private apiUrl = '/api/tours/api/location';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('userToken');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  getCurrentLocation(): Observable<TouristLocation | null> {
    return this.http.get<TouristLocation | null>(this.apiUrl, {
      headers: this.getAuthHeaders()
    });
  }

  setCurrentLocation(latitude: number, longitude: number): Observable<TouristLocation> {
    return this.http.post<TouristLocation>(
      this.apiUrl,
      { latitude, longitude },
      { headers: this.getAuthHeaders() }
    );
  }
}
