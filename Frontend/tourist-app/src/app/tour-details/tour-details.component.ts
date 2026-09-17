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
  isSavingReview = false;
  errorMessage = '';
  successMessage = '';
  reviewForm = {
    rating: 5,
    comment: '',
    visitDate: '',
    imageUrlsText: ''
  };

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

  createReview(): void {
    if (!this.tour) return;

    if (this.reviewForm.rating < 1 || this.reviewForm.rating > 5) {
      this.errorMessage = 'Ocena mora biti izmedju 1 i 5.';
      return;
    }

    if (!this.reviewForm.comment.trim() || !this.reviewForm.visitDate) {
      this.errorMessage = 'Unesi komentar i datum posete.';
      return;
    }

    this.isSavingReview = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.createReview(this.tour.id, {
      rating: this.reviewForm.rating,
      comment: this.reviewForm.comment,
      visitDate: this.reviewForm.visitDate,
      imageUrls: this.parseImageUrls()
    }).subscribe({
      next: review => {
        this.tour = {
          ...this.tour!,
          reviews: [...(this.tour!.reviews || []), review]
        };
        this.reviewForm = { rating: 5, comment: '', visitDate: '', imageUrlsText: '' };
        this.successMessage = 'Recenzija je uspesno dodata.';
        this.isSavingReview = false;
      },
      error: error => {
        this.errorMessage = error?.error?.message || error?.error || 'Nije moguce dodati recenziju.';
        this.isSavingReview = false;
      }
    });
  }

  deleteReview(reviewId: number): void {
    if (!this.tour) return;

    this.successMessage = '';
    this.errorMessage = '';

    this.tourService.deleteReview(this.tour.id, reviewId).subscribe({
      next: () => {
        this.tour = {
          ...this.tour!,
          reviews: (this.tour!.reviews || []).filter((review: any) => review.id !== reviewId)
        };
        this.successMessage = 'Recenzija je obrisana.';
      },
      error: error => {
        this.errorMessage = error?.error?.message || error?.error || 'Nije moguce obrisati recenziju.';
      }
    });
  }

  private parseImageUrls(): string[] {
    return this.reviewForm.imageUrlsText
      .split('\n')
      .map(url => url.trim())
      .filter(url => url.length > 0);
  }
}
