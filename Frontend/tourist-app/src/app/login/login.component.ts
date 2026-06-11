import { Component } from '@angular/core';
import { AuthService } from '../auth.service';
import { LoginDto } from '../auth.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginData: LoginDto = {
    username: '',
    password: ''
  };

  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) { }

  onLogin(): void {
    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        if (response && response.token === 'BLOCKED') {
          this.errorMessage = 'Vaš nalog je blokiran od strane administratora!';
          console.log('Prijava odbijena: Korisnik je blokiran.');
          return;
        }
        console.log('Uspešan login, token sačuvan!', response.token);
        this.errorMessage = '';
        
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Pogrešno korisničko ime ili lozinka.';
      }
    });
  }
}
