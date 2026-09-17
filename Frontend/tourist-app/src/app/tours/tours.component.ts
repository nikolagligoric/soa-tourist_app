import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TourPreview, TourService } from '../services/tour.service';
import { CartService } from '../services/cart.service';

@Component({
  selector: 'app-tours',
  templateUrl: './tours.component.html',
  styleUrls: ['./tours.component.css']
})
export class ToursComponent implements OnInit {
  tours: TourPreview[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private tourService: TourService,
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadTours();
  }

  loadTours(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.tourService.getPublishedTours().subscribe({
      next: (data) => {
        this.tours = data || [];
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Greška pri učitavanju tura';
        this.isLoading = false;
      }
    });
  }

  viewDetails(tourId: number): void {
    this.router.navigate(['/tours', tourId]);
  }

  addToCart(tourId: number): void {

    this.successMessage = '';
    this.errorMessage = '';

    this.cartService.addTourToCart(tourId).subscribe({

      next: () => {

        this.successMessage =
          'Tura je uspešno dodata u korpu.';

      },

      error: (error) => {

        this.errorMessage =
          error?.error?.message ||
          'Greška pri dodavanju ture.';

      }
    });
  }
}