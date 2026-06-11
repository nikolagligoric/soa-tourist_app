import { Component } from '@angular/core';
import { AuthService } from '../auth.service';
import { RegistrationDto } from '../auth.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
    registerData: RegistrationDto = {
    firstName: '',
    lastName: '',
    userName: '',
    password: '',
    email: '',
    role: 'Tourist' // Engleska reč, malo r
  };

  errorMessage: string = '';
  successMessage: string = '';

  constructor(private authService: AuthService, private router: Router) { }

  onRegister(): void {
    this.authService.register(this.registerData).subscribe({
      next: (response) => {
        console.log('Uspešna registracija:', response);
        this.successMessage = 'Uspešno ste se registrovali! Preusmeravanje na login...';
        this.errorMessage = '';
        
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Greška pri registraciji. Pokušajte ponovo.';
        this.successMessage = '';
      }
    });
  }
}