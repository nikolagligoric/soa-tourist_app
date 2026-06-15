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
  firstKeyPoint?: KeyPoint;
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

export interface CreateTourRequest {
  name: string;
  description: string;
  difficulty: string;
  tags: string;
}

export interface KeyPoint {
  id: number;
  name: string;
  description: string;
  latitude: number;
  longitude: number;
  sequence: number;
  imageUrl: string;
}

export interface CreateKeyPointRequest {
  name: string;
  description: string;
  latitude: number;
  longitude: number;
  imageUrl: string;
}

export interface TourDuration {
  id: number;
  transportType: string;
  durationMinutes: number;
}

export interface CreateTourDurationRequest {
  transportType: string;
  durationMinutes: number;
}

export interface TourReview {
  id: number;
  rating: number;
  comment: string;
  touristUsername: string;
  visitDate: string;
  commentDate: string;
  imageUrls: string[];
}

export interface CreateTourReviewRequest {
  rating: number;
  comment: string;
  visitDate: string;
  imageUrls: string[];
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

  createTour(request: CreateTourRequest): Observable<TourDetails> {
    return this.http.post<TourDetails>(this.apiUrl, {
      ...request,
      availableSlots: 1
    }, {
      headers: this.getHeaders()
    });
  }

  getToursByAuthor(username: string): Observable<TourDetails[]> {
    return this.http.get<TourDetails[]>(`${this.apiUrl}/author/${username}`, {
      headers: this.getHeaders()
    });
  }

  getKeyPoints(tourId: number): Observable<KeyPoint[]> {
    return this.http.get<KeyPoint[]>(`${this.apiUrl}/${tourId}/keypoints`, {
      headers: this.getHeaders()
    });
  }

  addKeyPoint(tourId: number, request: CreateKeyPointRequest): Observable<KeyPoint> {
    return this.http.post<KeyPoint>(`${this.apiUrl}/${tourId}/keypoints`, request, {
      headers: this.getHeaders()
    });
  }

  updateKeyPoint(tourId: number, keyPointId: number, request: CreateKeyPointRequest): Observable<KeyPoint> {
    return this.http.put<KeyPoint>(`${this.apiUrl}/${tourId}/keypoints/${keyPointId}`, request, {
      headers: this.getHeaders()
    });
  }

  deleteKeyPoint(tourId: number, keyPointId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${tourId}/keypoints/${keyPointId}`, {
      headers: this.getHeaders()
    });
  }

  addDuration(tourId: number, request: CreateTourDurationRequest): Observable<TourDuration> {
    return this.http.post<TourDuration>(`${this.apiUrl}/${tourId}/durations`, request, {
      headers: this.getHeaders()
    });
  }

  publishTour(tourId: number): Observable<TourDetails> {
    return this.http.put<TourDetails>(`${this.apiUrl}/${tourId}/publish`, {}, {
      headers: this.getHeaders()
    });
  }

  archiveTour(tourId: number): Observable<TourDetails> {
    return this.http.put<TourDetails>(`${this.apiUrl}/${tourId}/archive`, {}, {
      headers: this.getHeaders()
    });
  }

  reactivateTour(tourId: number): Observable<TourDetails> {
    return this.http.put<TourDetails>(`${this.apiUrl}/${tourId}/reactivate`, {}, {
      headers: this.getHeaders()
    });
  }

  createReview(tourId: number, request: CreateTourReviewRequest): Observable<TourReview> {
    return this.http.post<TourReview>(`${this.apiUrl}/${tourId}/reviews`, request, {
      headers: this.getHeaders()
    });
  }

  deleteReview(tourId: number, reviewId: number): Observable<string> {
    return this.http.delete(`${this.apiUrl}/${tourId}/reviews/${reviewId}`, {
      headers: this.getHeaders(),
      responseType: 'text'
    });
  }
}
