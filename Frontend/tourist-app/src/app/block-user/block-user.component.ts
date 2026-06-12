import { Component, OnInit } from '@angular/core';
import { AdminService } from '../services/admin.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-block-user',
  templateUrl: './block-user.component.html',
  styleUrls: ['./block-user.component.css']
})
export class BlockUserComponent implements OnInit {
  users: any[] = [];
  errorMessage: string = '';
  successMessage: string = '';

  constructor(
    private adminService: AdminService, 
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    const user = this.authService.getUserDetails();
    if (!user || (user.role !== 'Administrator' && user.role !== 'Admin')) {
      this.router.navigate(['/home']);
      return;
    }

    this.loadUsers();
  }

  loadUsers(): void {
    this.adminService.getAllUsers().subscribe({
      next: (data) => {
        this.users = data;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Greška pri učitavanju korisnika.';
      }
    });
  }

  onBlock(userId: number): void {
    if (confirm('Da li ste sigurni da želite da blokirate ovog korisnika?')) {
      this.adminService.blockUser(userId).subscribe({
        next: () => {
          this.successMessage = 'Korisnik je uspešno blokiran.';
          this.errorMessage = '';
          this.loadUsers();
        },
        error: (err) => {
          console.error(err);
          this.errorMessage = 'Greška pri blokiranju korisnika.';
          this.successMessage = '';
        }
      });
    }
  }
}