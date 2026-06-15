import { Component, OnInit } from '@angular/core';
import { CartItem, CartService, ShoppingCart } from '../services/cart.service';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cart: ShoppingCart | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(private cartService: CartService) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.cartService.getCart().subscribe({
      next: (data) => {
        this.cart = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Greška pri učitavanju korpe.';
        this.isLoading = false;
      }
    });
  }

  removeItem(item: CartItem): void {
    this.successMessage = '';
    this.errorMessage = '';

    this.cartService.removeItem(item.id).subscribe({
      next: (data) => {
        this.cart = data;
        this.successMessage = 'Tura uklonjena iz korpe.';
      },
      error: () => {
        this.errorMessage = 'Greška pri uklanjanju ture iz korpe.';
      }
    });
  }

  checkout(): void {
    this.successMessage = '';
    this.errorMessage = '';

    this.cartService.checkout().subscribe({
      next: () => {
        this.successMessage = 'Plaćanje uspešno završeno.';
        this.loadCart();
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message || 'Plaćanje nije uspelo.';
      }
    });
  }

  getItemName(item: CartItem): string {
    return item.tourName || item.name || `Tura #${item.tourId}`;
  }
}