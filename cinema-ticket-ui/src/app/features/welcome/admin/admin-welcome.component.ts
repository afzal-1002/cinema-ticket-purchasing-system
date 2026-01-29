import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserSummary } from '../../../core/models/auth.model';

@Component({
  selector: 'app-admin-welcome',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-welcome.component.html',
  styleUrls: ['./admin-welcome.component.css']
})
export class AdminWelcomeComponent {
  user: UserSummary | null;

  constructor(private authService: AuthService, private router: Router) {
    this.user = this.authService.getStoredUser();
  }

  get displayName(): string {
    if (this.user?.firstName) {
      return `${this.user.firstName}${this.user.lastName ? ' ' + this.user.lastName : ''}`.trim();
    }
    return 'Admin';
  }

  get email(): string {
    return this.user?.email ?? 'admin@cinema.local';
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

  goToScreenings(): void {
    this.router.navigate(['/screenings']);
  }

  goToMovies(): void {
    this.router.navigate(['/movies']);
  }

  goToUsers(): void {
    this.router.navigate(['/admin/users']);
  }
}
