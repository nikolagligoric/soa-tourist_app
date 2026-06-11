import { Component, OnInit } from '@angular/core';
import { AdminService } from '../admin.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileData: any = {};
  successMessage: string = '';
  errorMessage: string = '';

  constructor(private profileService: AdminService) { }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.profileService.getMyProfile().subscribe({
      next: (data) => {
        this.profileData = data;
        if (!this.profileData.profileImageUrl) {
          this.profileData.profileImageUrl = '';
        }
        console.log('Profil učitan.');
      },
      error: (err) => {
        this.errorMessage = 'Nije moguće učitati profil.';
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 2 * 1024 * 1024) {
        alert('Slika je prevelika. Maksimalna veličina je 2MB.');
        return;
      }

      const reader = new FileReader();
      
      reader.readAsDataURL(file);
      
      reader.onload = () => {
        this.profileData.profileImageUrl = reader.result as string;
        console.log('Slika konvertovana u Base64.');
      };
      
      reader.onerror = (error) => {
        console.error('Greška pri čitanju fajla:', error);
      };
    }
  }

  onUpdateProfile(): void {
    this.successMessage = '';
    this.errorMessage = '';

    const cleanData = { ...this.profileData };
    if (!cleanData.profileImageUrl || cleanData.profileImageUrl.trim() === '') cleanData.profileImageUrl = null;
    if (!cleanData.motto || cleanData.motto.trim() === '') cleanData.motto = null;
    if (!cleanData.bio || cleanData.bio.trim() === '') cleanData.bio = null;

    this.profileService.updateMyProfile(cleanData).subscribe({
      next: (response) => {
        this.successMessage = 'Profil je uspešno ažuriran!';
        console.log(response);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = err.error || 'Greška pri čuvanju izmena.';
      }
    });
  }
}