import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TourDetails, TourService } from '../services/tour.service';
import { CartService } from '../services/cart.service';

@Component({
  selector: 'app-tour-details',
  templateUrl: './tour-details.component.html',
  styleUrls: ['./tour-details.component.css']
})
export class TourDetailsComponent implements OnInit {
  tour: TourDetails | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private tourService: TourService,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.router.navigate(['/tours']);
      return;
    }

    this.loadTour(id);
  }

  loadTour(id: number): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.tourService.getTourDetails(id).subscribe({
      next: (data) => {
        this.tour = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Greška pri učitavanju detalaj ture.';
        this.isLoading = false;
      }
    });
  }

  addToCart(): void {
    if (!this.tour) return;

    this.successMessage = '';
    this.errorMessage = '';

    this.cartService.addTourToCart(this.tour.id).subscribe({
      next: () => {
        this.successMessage = 'Tura uspešno dodata u korpu.';
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Greška pri dodavanju ture  u korpu.';
      }
    });
  }
    
  goBack(): void {
    this.router.navigate(['/tours']);
  }

  startTour(): void {
    if (!this.tour) return;

    this.router.navigate(['/active-tour'], {
      queryParams: { tourId: this.tour.id }
    });
  }
}