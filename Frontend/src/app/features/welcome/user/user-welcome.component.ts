import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserSummary } from '../../../core/models/auth.model';

@Component({
  selector: 'app-user-welcome',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-welcome.component.html',
  styleUrls: ['./user-welcome.component.css']
})
export class UserWelcomeComponent {
  user: UserSummary | null;

  constructor(private authService: AuthService, private router: Router) {
    this.user = this.authService.getStoredUser();
  }

  get displayName(): string {
    if (this.user?.firstName) {
      return `${this.user.firstName}${this.user.lastName ? ' ' + this.user.lastName : ''}`.trim();
    }
    return 'Movie Lover';
  }

  get email(): string {
    return this.user?.email ?? 'guest@cinema.local';
  }

  logout(): void {
    this.authService.clearSession();
    this.router.navigate(['/login']);
  }

  goToProfile(): void {
    this.router.navigate(['/profile']);
  }

  goToCinemas(): void {
    this.router.navigate(['/cinemas']);
  }
}
