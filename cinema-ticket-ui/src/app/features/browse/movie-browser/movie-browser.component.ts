import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Movie } from '../../../core/models/movie.model';
import { ScreeningSummary } from '../../../core/models/screening.model';
import { MovieService } from '../../../core/services/movie.service';
import { ScreeningService } from '../../../core/services/screening.service';

interface MovieAvailability {
  movie: Movie;
  screenings: ScreeningSummary[];
  upcomingScreenings: ScreeningSummary[];
  totalAvailableSeats: number;
  totalReservedSeats: number;
  totalHeldSeats: number;
  nextScreening: ScreeningSummary | null;
}

@Component({
  selector: 'app-movie-browser',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './movie-browser.component.html',
  styleUrls: ['./movie-browser.component.css']
})
export class MovieBrowserComponent implements OnInit {
  availabilities: MovieAvailability[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  searchTerm = '';

  constructor(
    private movieService: MovieService,
    private screeningService: ScreeningService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMovies();
  }

  loadMovies(): void {
    this.isLoading = true;
    this.errorMessage = null;

    forkJoin({
      movies: this.movieService.getActiveMovies(),
      screenings: this.screeningService.getScreenings()
    }).subscribe({
      next: ({ movies, screenings }) => {
        const now = Date.now();
        this.availabilities = movies
          .map((movie) => {
            const linkedScreenings = screenings.filter((screening) => screening.movieId === movie.id);
            const upcomingScreenings = linkedScreenings
              .filter((screening) => new Date(screening.startDateTime).getTime() >= now)
              .sort((a, b) => new Date(a.startDateTime).getTime() - new Date(b.startDateTime).getTime());

            const totalAvailableSeats = linkedScreenings.reduce(
              (total, screening) => total + (screening.availableSeats ?? 0),
              0
            );
            const totalReservedSeats = linkedScreenings.reduce(
              (total, screening) => total + (screening.reservedSeats ?? 0),
              0
            );
            const totalHeldSeats = linkedScreenings.reduce(
              (total, screening) => total + (screening.heldSeats ?? 0),
              0
            );

            return {
              movie,
              screenings: linkedScreenings,
              upcomingScreenings,
              totalAvailableSeats,
              totalReservedSeats,
              totalHeldSeats,
              nextScreening: upcomingScreenings[0] ?? null
            };
          })
          .filter((entry) => entry.movie.isActive !== false)
          .sort((a, b) => a.movie.title.localeCompare(b.movie.title));

        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load movie availability at the moment.';
        this.isLoading = false;
      }
    });
  }

  filteredAvailabilities(): MovieAvailability[] {
    if (!this.searchTerm.trim()) {
      return this.availabilities;
    }
    const term = this.searchTerm.trim().toLowerCase();
    return this.availabilities.filter((entry) =>
      entry.movie.title.toLowerCase().includes(term) ||
      (entry.movie.genre ?? '').toLowerCase().includes(term)
    );
  }

  getTotalAvailableSeats(): number {
    return this.filteredAvailabilities().reduce(
      (total, entry) => total + entry.totalAvailableSeats,
      0
    );
  }

  bookFromMovie(entry: MovieAvailability): void {
    if (entry.nextScreening) {
      this.router.navigate(['/browse/screenings', entry.nextScreening.id, 'seats']);
    } else {
      this.router.navigate(['/browse/screenings']);
    }
  }

  goToScreenings(): void {
    this.router.navigate(['/browse/screenings']);
  }

  goHome(): void {
    this.router.navigate(['/user/welcome']);
  }
}
