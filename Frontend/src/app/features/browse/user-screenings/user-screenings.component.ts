import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ScreeningService } from '../../../core/services/screening.service';
import { ScreeningSummary } from '../../../core/models/screening.model';

@Component({
  selector: 'app-user-screenings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-screenings.component.html',
  styleUrls: ['./user-screenings.component.css']
})
export class UserScreeningsComponent implements OnInit {
  screenings: ScreeningSummary[] = [];
  allScreenings: ScreeningSummary[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  cinemaOptions: string[] = [];

  searchQuery = '';
  cinemaFilter = 'all';
  minAvailable = 0;
  sortMode: 'soonest' | 'latest' | 'seats-desc' | 'price-asc' | 'price-desc' = 'soonest';

  constructor(
    private screeningService: ScreeningService,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.fetchScreenings();
  }

  fetchScreenings(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.screeningService.getScreenings().subscribe({
      next: (data) => {
        this.allScreenings = data;
        this.cinemaOptions = Array.from(new Set(data.map((s) => s.cinemaName))).sort();
        this.applyFilters();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load screenings right now.';
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    let result = [...this.allScreenings];

    if (this.cinemaFilter !== 'all') {
      result = result.filter((screening) => screening.cinemaName === this.cinemaFilter);
    }

    if (this.searchQuery.trim().length) {
      const term = this.searchQuery.trim().toLowerCase();
      result = result.filter(
        (screening) =>
          screening.movieTitle.toLowerCase().includes(term) ||
          screening.cinemaName.toLowerCase().includes(term)
      );
    }

    if (this.minAvailable > 0) {
      result = result.filter((screening) => screening.availableSeats >= this.minAvailable);
    }

    switch (this.sortMode) {
      case 'latest':
        result.sort(
          (a, b) =>
            new Date(b.startDateTime).getTime() - new Date(a.startDateTime).getTime()
        );
        break;
      case 'seats-desc':
        result.sort((a, b) => b.availableSeats - a.availableSeats);
        break;
      case 'price-asc':
        result.sort((a, b) => a.ticketPrice - b.ticketPrice);
        break;
      case 'price-desc':
        result.sort((a, b) => b.ticketPrice - a.ticketPrice);
        break;
      default:
        result.sort(
          (a, b) =>
            new Date(a.startDateTime).getTime() - new Date(b.startDateTime).getTime()
        );
        break;
    }

    this.screenings = result;
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.cinemaFilter = 'all';
    this.minAvailable = 0;
    this.sortMode = 'soonest';
    this.applyFilters();
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  selectSeats(screening: ScreeningSummary): void {
    this.router.navigate(['/browse/screenings', screening.id, 'seats']);
  }

  trackById(_index: number, screening: ScreeningSummary): number {
    return screening.id;
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/user/welcome']);
  }

  goToMovies(): void {
    this.router.navigate(['/browse/movies']);
  }
}
