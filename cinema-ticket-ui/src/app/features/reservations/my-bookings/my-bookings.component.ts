import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ReservationService } from '../../../core/services/reservation.service';
import { ReservationSummary } from '../../../core/models/reservation.model';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-bookings.component.html',
  styleUrls: ['./my-bookings.component.css']
})
export class MyBookingsComponent implements OnInit {
  reservations: ReservationSummary[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  cancellingId: number | null = null;

  constructor(
    private reservationService: ReservationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    this.reservationService.getMyReservations().subscribe({
      next: (data) => {
        this.reservations = [...data].sort(
          (a, b) => new Date(a.startDateTime).getTime() - new Date(b.startDateTime).getTime()
        );
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load your bookings right now. Please try again shortly.';
        this.isLoading = false;
      }
    });
  }

  cancelReservation(reservation: ReservationSummary): void {
    if (this.cancellingId === reservation.id) {
      return;
    }

    this.cancellingId = reservation.id;
    this.errorMessage = null;
    this.successMessage = null;

    this.reservationService
      .cancelReservation(reservation.screeningId, reservation.row, reservation.seat)
      .subscribe({
        next: () => {
          this.reservations = this.reservations.filter((entry) => entry.id !== reservation.id);
          this.successMessage = 'Booking cancelled successfully.';
          this.cancellingId = null;
        },
        error: () => {
          this.errorMessage = 'Could not cancel this booking. Please try again.';
          this.cancellingId = null;
        }
      });
  }

  isCancelling(reservationId: number): boolean {
    return this.cancellingId === reservationId;
  }

  goHome(): void {
    this.router.navigate(['/user/welcome']);
  }

  goToMovies(): void {
    this.router.navigate(['/browse/movies']);
  }
}
