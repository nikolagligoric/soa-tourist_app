import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  UrlTree
} from '@angular/router';

import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {

  constructor(
    private router: Router
  ) {}

  canActivate(): boolean | UrlTree {

    const token =
      localStorage.getItem('userToken');

    if (!token) {
      return this.router.createUrlTree(['/login']);
    }

    try {

      const decoded: any =
        jwtDecode(token);

      const role =
        decoded[
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        ] ||
        decoded['role'];

      const isAdmin =
        role === 'Admin' ||
        role === 'Administrator';

      if (isAdmin) {
        return true;
      }

      return this.router.createUrlTree(['/home']);

    } catch {

      localStorage.removeItem('userToken');

      return this.router.createUrlTree(['/login']);
    }
  }
}