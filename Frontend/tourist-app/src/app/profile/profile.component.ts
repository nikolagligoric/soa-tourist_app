import { Component, OnInit } from '@angular/core';
import { FollowersService } from '../services/followers.service';
import { jwtDecode } from 'jwt-decode';

import { AdminService } from '../services/admin.service'; 

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileData: any = {};
  successMessage: string = '';
  errorMessage: string = '';
  isEditMode: boolean = false;

  followingCount: number = 0;
  followersCount: number = 0;
  currentUsername: string = '';

  rawFollowingList: string[] = [];
  rawFollowersList: string[] = [];
  activeModalList: string[] = [];
  modalTitle: string = '';
  modalType: 'followers' | 'following' = 'following';
  followingMap: { [key: string]: boolean } = {};

  constructor(
    private profileService: AdminService,
    private followersService: FollowersService 
  ) { }

  ngOnInit(): void {
    this.extractUsernameFromToken();
    this.loadProfile();
    this.loadFollowersStats();
  }

  extractUsernameFromToken(): void {
    const token = localStorage.getItem('userToken');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.currentUsername = decoded.username || decoded.name || decoded.sub || 'korisnik';
      } catch (error) {
        console.error('Greška pri dekodiranju tokena:', error);
      }
    }
  }

  loadProfile(): void {
    this.profileService.getMyProfile().subscribe({
      next: (data: any) => {
        this.profileData = data;
        if (!this.profileData.profileImageUrl) {
          this.profileData.profileImageUrl = '';
        }
      },
      error: (err: any) => {
        this.errorMessage = 'Nije moguće učitati profil.';
      }
    });
  }

  loadFollowersStats(): void {
    this.followersService.getFollowing().subscribe({
      next: (followingUsers: string[]) => {
        this.rawFollowingList = followingUsers;
        this.followingCount = followingUsers.length;
        
        this.followingMap = {};
        followingUsers.forEach(username => {
          this.followingMap[username] = true;
        });

        if (this.modalType === 'following') {
          this.activeModalList = followingUsers;
        }
      },
      error: (err: any) => console.error('Greška pri preuzimanju praćenja:', err)
    });

    this.followersService.getFollowers().subscribe({
      next: (followersUsers: string[]) => {
        this.rawFollowersList = followersUsers;
        this.followersCount = followersUsers.length;
        if (this.modalType === 'followers') {
          this.activeModalList = followersUsers;
        }
      },
      error: (err: any) => console.error('Greška pri preuzimanju pratilaca:', err)
    });
  }

  openModal(type: 'followers' | 'following'): void {
    this.modalType = type;
    this.modalTitle = type === 'following' ? 'Korisnici koje pratiš' : 'Tvoji pratioci';
    this.activeModalList = type === 'following' ? this.rawFollowingList : this.rawFollowersList;

    const modal = document.getElementById('followersModal') as HTMLDialogElement;
    if (modal) {
      modal.showModal();
    }
  }

  closeModal(): void {
    const modal = document.getElementById('followersModal') as HTMLDialogElement;
    if (modal) {
      modal.close();
    }
  }

  followFromList(usernameToFollow: string): void {
    this.followersService.follow(usernameToFollow).subscribe({
      next: () => {
        this.loadFollowersStats();
      },
      error: (err: any) => console.error('Greška pri zapraćivanju:', err)
    });
  }

  unfollowFromList(usernameToUnfollow: string): void {
    this.followersService.unfollow(usernameToUnfollow).subscribe({
      next: () => {
        this.loadFollowersStats();
      },
      error: (err: any) => {
        console.error('Greška pri otpraćivanju korisnika:', err);
      }
    });
  }

  toggleEditMode(mode: boolean): void {
    this.isEditMode = mode;
    this.successMessage = '';
    this.errorMessage = '';
    if (!mode) {
      this.loadProfile();
    }
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
      next: (response: any) => {
        this.successMessage = 'Profil je uspešno ažuriran!';
        this.isEditMode = false;
        this.loadProfile();
      },
      error: (err: any) => {
        this.errorMessage = err.error || 'Greška pri čuvanju izmena.';
      }
    });
  }
}