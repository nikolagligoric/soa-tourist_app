import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  currentUserRole: string | null = null;

  constructor(private router: Router) {}

  isLoggedIn(): boolean {
    const token = localStorage.getItem('userToken');
    
    if (token) {
      if (!this.currentUserRole) {
        this.checkUserRole(token);
      }
      return true;
    }
    
    this.currentUserRole = null;
    return false;
  }

  checkUserRole(token: string): void {
    try {
      const decoded: any = jwtDecode(token);
      this.currentUserRole = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded['role'];
      console.log('Rola uspešno prepoznata u hodu:', this.currentUserRole);
    } catch (error) {
      console.error('Greška pri dekodiranju tokena u navbaru:', error);
      this.currentUserRole = null;
    }
  }

  onLogout(): void {
    localStorage.removeItem('userToken');
    this.currentUserRole = null;
    console.log('Korisnik uspešno odjavljen.');
    this.router.navigate(['/login']);
  }
}