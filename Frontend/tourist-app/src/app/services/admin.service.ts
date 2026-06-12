import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = '/api/stakeholders/api/users';

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('userToken');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  getAllUsers(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl, { headers: this.getAuthHeaders() });
  }

  blockUser(userId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${userId}/block`, {}, { headers: this.getAuthHeaders() });
  }

  getMyProfile(): Observable<any> {
    return this.http.get<any>('/api/stakeholders/api/users/profile', { headers: this.getAuthHeaders() });
  }

  updateMyProfile(profileData: any): Observable<any> {
    return this.http.put<any>('/api/stakeholders/api/users/profile', profileData, { headers: this.getAuthHeaders() });
  }

  getProfileByUsername(username: string): Observable<any> {
    return this.http.get<any>(`/api/stakeholders/api/users/profile/${username}`, { headers: this.getAuthHeaders() });
  }
}