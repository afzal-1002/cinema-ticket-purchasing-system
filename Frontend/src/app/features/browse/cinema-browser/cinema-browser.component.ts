import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CinemaSummary } from '../../../core/models/cinema.model';
import { ScreeningSummary } from '../../../core/models/screening.model';
import { CinemaService } from '../../../core/services/cinema.service';
import { ScreeningService } from '../../../core/services/screening.service';

interface CinemaAvailability {
  cinema: CinemaSummary;
  totalSeats: number;
  nextScreening: ScreeningSummary | null;
}

@Component({
  selector: 'app-cinema-browser',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cinema-browser.component.html',
  styleUrls: ['./cinema-browser.component.css']
})
export class CinemaBrowserComponent implements OnInit {
  availabilities: CinemaAvailability[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  lastUpdated: Date | null = null;

  constructor(
    private cinemaService: CinemaService,
    private screeningService: ScreeningService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAvailability();
  }

  loadAvailability(): void {
    this.isLoading = true;
    this.errorMessage = null;

    forkJoin({
      cinemas: this.cinemaService.getCinemas(),
      screenings: this.screeningService.getScreenings()
    }).subscribe({
      next: ({ cinemas, screenings }) => {
        const now = Date.now();
        this.availabilities = cinemas
          .map((cinema) => {
            const totalSeats = cinema.totalSeats ?? (cinema.rows ?? 0) * (cinema.seatsPerRow ?? 0);
            const upcoming = screenings
              .filter(
                (screening) =>
                  screening.cinemaId === cinema.id &&
                  new Date(screening.startDateTime).getTime() >= now
              )
              .sort(
                (a, b) =>
                  new Date(a.startDateTime).getTime() - new Date(b.startDateTime).getTime()
              )[0] || null;

            return { cinema, totalSeats, nextScreening: upcoming };
          })
          .sort((a, b) => a.cinema.name.localeCompare(b.cinema.name));

        this.lastUpdated = new Date();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load cinemas right now. Please try again in a moment.';
        this.isLoading = false;
      }
    });
  }

  trackByCinemaIndex(_index: number, entry: CinemaAvailability): number {
    return entry.cinema.id;
  }

  bookTickets(entry: CinemaAvailability): void {
    if (entry.nextScreening) {
      this.router.navigate(['/browse/screenings', entry.nextScreening.id, 'seats']);
    } else {
      this.router.navigate(['/browse/screenings']);
    }
  }

  goToAllScreenings(): void {
    this.router.navigate(['/browse/screenings']);
  }
}
