import { Injectable } from '@angular/core';
import { HttpClient as AngularHttp } from '@angular/common/http';
import { Observable, tap, Subject } from 'rxjs';
import { LoginDto, RegistrationDto } from '../auth.models';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:8000/stakeholders/api/users';

  authStatusChange = new Subject<void>();

  constructor(private http: AngularHttp) { }

  login(loginDto: LoginDto): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/login`, loginDto).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('userToken', response.token);
          this.authStatusChange.next();
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('userToken');
    this.authStatusChange.next();
  }

  register(registerDto: RegistrationDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, registerDto);
  }

  getUserDetails(): any {
    const token = localStorage.getItem('userToken');
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);
      
      return {
        id: decoded['id'] || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        username: decoded['username'] || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        role: decoded['role'] || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      };
    } catch (error) {
      console.error('Greška pri dekodiranju tokena:', error);
      return null;
    }
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('userToken');
  }

}