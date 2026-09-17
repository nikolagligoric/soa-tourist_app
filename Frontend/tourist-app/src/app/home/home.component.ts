import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { FollowersService } from '../services/followers.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  currentUser: any = null;
  recommendedUsers: string[] = [];

  constructor(private authService: AuthService, private followersService: FollowersService, private router: Router) { }

  ngOnInit(): void {
    this.currentUser = this.authService.getUserDetails();
    if (!this.currentUser) { this.router.navigate(['/login']); return; }
    if (this.currentUser.role !== 'Admin' && this.currentUser.role !== 'Administrator') this.loadRecommendations();
  }

  loadRecommendations(): void {
    this.followersService.getRecommendations().subscribe(users => this.recommendedUsers = users);
  }

  followFromHome(username: string): void {
    this.followersService.follow(username).subscribe(() => {
      this.recommendedUsers = this.recommendedUsers.filter(u => u !== username);
    });
  }
}