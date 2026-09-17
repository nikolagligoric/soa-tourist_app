import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FollowersService {
  private apiUrl = 'http://localhost:8000/api/followers'; 

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('userToken'); 
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  getFollowing(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/following`, { headers: this.getAuthHeaders() });
  }

  getFollowers(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/followers`, { headers: this.getAuthHeaders() });
  }

  follow(username: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${username}/follow`, {}, { headers: this.getAuthHeaders() });
  }

  unfollow(username: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${username}/unfollow`, { headers: this.getAuthHeaders() });
  }

  checkFollowing(username: string): Observable<{ isFollowing: boolean }> {
    return this.http.get<{ isFollowing: boolean }>(`${this.apiUrl}/check/${username}`, { headers: this.getAuthHeaders() });
  }

  getRecommendations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/recommendations`, { headers: this.getAuthHeaders() });
  }
}