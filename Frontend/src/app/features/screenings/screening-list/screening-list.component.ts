import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ScreeningService } from '../../../core/services/screening.service';
import { ScreeningSummary } from '../../../core/models/screening.model';

@Component({
  selector: 'app-screening-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './screening-list.component.html',
  styleUrls: ['./screening-list.component.css']
})
export class ScreeningListComponent implements OnInit {
  screenings: ScreeningSummary[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private screeningService: ScreeningService,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.loadScreenings();
  }

  loadScreenings(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.screeningService.getScreenings().subscribe({
      next: (data) => {
        this.screenings = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load screenings. Please try again later.';
        this.isLoading = false;
      }
    });
  }

  viewDetails(screening: ScreeningSummary): void {
    this.router.navigate(['/screenings', screening.id]);
  }

  confirmDelete(screening: ScreeningSummary): void {
    const shouldDelete = window.confirm(
      `Delete screening for ${screening.movieTitle} at ${screening.cinemaName}? All reservations will be removed.`
    );
    if (!shouldDelete) {
      return;
    }

    this.screeningService.deleteScreening(screening.id).subscribe({
      next: () => this.loadScreenings(),
      error: () => {
        this.errorMessage = 'Failed to delete screening. Please refresh and try again.';
      }
    });
  }

  trackById(_index: number, screening: ScreeningSummary): number {
    return screening.id;
  }

  goBack(): void {
    this.location.back();
  }
}
