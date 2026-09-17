import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminService } from '../services/admin.service'; 
import { FollowersService } from '../services/followers.service';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css']
})
export class UserProfileComponent implements OnInit {
  username: string = '';
  userData: any = {};
  isFollowing: boolean = false;
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private profileService: AdminService,
    private followersService: FollowersService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.username = params['username'];
      this.loadUserProfile();
      this.checkIfFollowing();
    });
  }

  loadUserProfile(): void {
    this.profileService.getProfileByUsername(this.username).subscribe({
      next: (data: any) => {
        this.userData = data;
      },
      error: (err: any) => {
        this.errorMessage = 'Korisnik nije pronađen.';
      }
    });
  }

  checkIfFollowing(): void {
    this.followersService.checkFollowing(this.username).subscribe({
      next: (res: any) => {
        this.isFollowing = res.isFollowing;
      },
      error: (err: any) => console.error('Greška pri proveri praćenja:', err)
    });
  }

  toggleFollow(): void {
    if (this.isFollowing) {
      this.followersService.unfollow(this.username).subscribe({
        next: () => {
          this.isFollowing = false;
        },
        error: (err: any) => console.error('Greška pri otpraćivanju:', err)
      });
    } else {
      this.followersService.follow(this.username).subscribe({
        next: () => {
          this.isFollowing = true;
        },
        error: (err: any) => console.error('Greška pri zapraćivanju:', err)
      });
    }
  }
}