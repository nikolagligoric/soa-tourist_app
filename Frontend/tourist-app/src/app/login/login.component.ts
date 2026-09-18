import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { MonitoringService } from '../services/monitoring.service';
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

  errorMessage = '';
  showMonitoringAlert = false;

  constructor(
    private authService: AuthService,
    private monitoringService: MonitoringService,
    private router: Router
  ) { }

  onLogin(): void {
    this.errorMessage = '';

    this.authService.login(this.loginData).subscribe({
      next: (response) => {

        if (
          response &&
          response.token === 'BLOCKED'
        ) {
          this.errorMessage =
            'Vaš nalog je blokiran od strane administratora!';

          return;
        }

        this.errorMessage = '';
        this.showMonitoringAlert = false;

        this.router.navigate(['/home']);
      },

      error: (err) => {
        console.error(err);

        this.errorMessage =
          'Pogrešno korisničko ime ili lozinka.';

        setTimeout(() => {
          this.checkLoginAlert();
        }, 5000);
      }
    });
  }

  private checkLoginAlert(): void {
    this.monitoringService
      .getActiveAlerts()
      .subscribe({
        next: (alerts) => {

          const loginAlert =
            alerts.find(alert =>
              alert.serviceName === 'Gateway' &&
              alert.type === 'FAILED_LOGIN_ATTEMPTS' &&
              alert.status === 'ACTIVE'
            );

          if (loginAlert) {
            this.showMonitoringAlert = true;
          }

        },

        error: (error) => {
          console.error(
            'Nije moguće proveriti monitoring alerte.',
            error
          );
        }
      });
  }

  closeMonitoringAlert(): void {
    this.showMonitoringAlert = false;
  }

}