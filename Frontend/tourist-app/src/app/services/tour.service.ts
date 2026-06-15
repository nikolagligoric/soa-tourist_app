import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TourPreview {
  id: number;
  name: string;
  description: string;
  difficulty: string;
  price: number;
  tags: string;
  distanceInKm: number;
  authorUsername: string;
}

export interface TourDetails {
  id: number;
  name: string;
  description: string;
  difficulty: string;
  price: number;
  status: string;
  tags: string;
  distanceInKm: number;
  authorUsername: string;
  keyPoints?: any[];
  durations?: any[];
  reviews?: any[];
  purchased?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TourService {
  private apiUrl = '/tour-api';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('userToken');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  getPublishedTours(): Observable<TourPreview[]> {
    return this.http.get<TourPreview[]>(`${this.apiUrl}/published`, {
      headers: this.getHeaders()
    });
  }

  getTourDetails(id: number): Observable<TourDetails> {
    return this.http.get<TourDetails>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }
}