import { Component, OnInit, HostListener, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
  currentUserRole: string | null = null;
  currentUserUsername: string = '';
  isDropdownOpen: boolean = false;
  loggedInStatus: boolean = false;

  constructor(
    private router: Router, 
    private eRef: ElementRef,
    private authService: AuthService
  ) {}

  get isLoggedIn(): boolean {
    return this.loggedInStatus;
  }

  ngOnInit(): void {
    this.checkUserAuthentication();
    this.authService.authStatusChange.subscribe(() => {
      this.checkUserAuthentication();
    });
  }

  checkUserAuthentication(): void {
    const token = localStorage.getItem('userToken');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.currentUserRole = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded['role'];
        this.currentUserUsername = decoded['username'] || 'Korisnik';
        this.loggedInStatus = true;
      } catch (e) { this.logoutDataClean(); }
    }
  }

  logoutDataClean(): void {
    this.currentUserRole = null;
    this.currentUserUsername = '';
    this.loggedInStatus = false;
  }

  toggleDropdown(event: Event): void {
    event.stopPropagation();
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.contains(event.target)) this.isDropdownOpen = false;
  }

  onLogout(): void {
    localStorage.removeItem('userToken');
    this.logoutDataClean();
    this.router.navigate(['/login']);
  }
}