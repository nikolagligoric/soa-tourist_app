import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CartItem {
  id: number;
  tourId: number;
  tourName?: string;
  name?: string;
  price: number;
}

export interface ShoppingCart {
  id: number;
  touristUsername: string;
  totalPrice: number;
  items: CartItem[];
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = '/cart-api';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('userToken');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  addTourToCart(tourId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/add/${tourId}`, {}, {
      headers: this.getHeaders()
    });
  }

  getCart(): Observable<ShoppingCart> {
    return this.http.get<ShoppingCart>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  removeItem(itemId: number): Observable<ShoppingCart> {
    return this.http.delete<ShoppingCart>(`${this.apiUrl}/items/${itemId}`, {
      headers: this.getHeaders()
    });
  }

  checkout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/checkout`, {}, {
      headers: this.getHeaders()
    });
  }

  hasPurchased(tourId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/has-purchased/${tourId}`, {
      headers: this.getHeaders()
    });
  }

  getPurchasedTours(username: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/purchased/${username}`, {
      headers: this.getHeaders()
    });
  }
}