import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { CartService } from '../services/cart.service';

@Component({
  selector: 'app-purchases',
  templateUrl: './purchases.component.html',
  styleUrls: ['./purchases.component.css']
})
export class PurchasesComponent implements OnInit {
  purchasedTours: any[] = [];
  username = '';
  isLoading = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const user = this.authService.getUserDetails();

    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    this.username = user.username;
    this.loadPurchasedTours();
  }

  loadPurchasedTours(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.cartService.getPurchasedTours(this.username).subscribe({
      next: (data) => {
        this.purchasedTours = data || [];
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Greška pri učitavanju kupljenih tura.';
        this.isLoading = false;
      }
    });
  }

  viewDetails(tour: any): void {
    const tourId = tour.tourId || tour.id;
    this.router.navigate(['/tours', tourId]);
  }

  startTour(tour: any): void {
    const tourId = tour.tourId || tour.id;
    this.router.navigate(['/active-tour'], {
      queryParams: { tourId }
    });
  }

  getTourName(tour: any): string {
    return tour.tourName || tour.name || `Tura #${tour.tourId || tour.id}`;
  }

  getTourPrice(tour: any): number {
    return tour.price || tour.tourPrice || 0;
  }
}