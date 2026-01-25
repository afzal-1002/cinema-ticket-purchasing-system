import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ScreeningService } from '../../../core/services/screening.service';
import { ScreeningSummary } from '../../../core/models/screening.model';

@Component({
  selector: 'app-user-screenings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-screenings.component.html',
  styleUrls: ['./user-screenings.component.css']
})
export class UserScreeningsComponent implements OnInit {
  screenings: ScreeningSummary[] = [];
  isLoading = false;
  errorMessage: string | null = null;

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
        this.screenings = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load screenings right now.';
        this.isLoading = false;
      }
    });
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
}
