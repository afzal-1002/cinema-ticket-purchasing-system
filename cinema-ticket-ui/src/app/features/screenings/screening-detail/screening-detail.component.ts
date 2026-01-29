import { CommonModule, Location } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ScreeningDetail, SeatSummary } from '../../../core/models/screening.model';
import { ScreeningService } from '../../../core/services/screening.service';

@Component({
  selector: 'app-screening-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './screening-detail.component.html',
  styleUrls: ['./screening-detail.component.css']
})
export class ScreeningDetailComponent implements OnInit, OnDestroy {
  screening: ScreeningDetail | null = null;
  seatRows: SeatSummary[][] = [];
  isLoading = false;
  errorMessage: string | null = null;
  private subscription: Subscription | null = null;
  private paramSubscription: Subscription | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private screeningService: ScreeningService,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.paramSubscription = this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!id) {
        this.errorMessage = 'Invalid screening ID.';
        return;
      }
      this.fetchScreening(id);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.paramSubscription?.unsubscribe();
  }

  fetchScreening(id: number): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.subscription = this.screeningService.getScreeningById(id).subscribe({
      next: (detail) => {
        this.screening = detail;
        this.seatRows = this.buildSeatRows(detail);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load screening. It may have been removed.';
        this.isLoading = false;
      }
    });
  }

  private buildSeatRows(detail: ScreeningDetail): SeatSummary[][] {
    const rows: SeatSummary[][] = [];
    if (!detail.rows || !detail.seatsPerRow || !detail.seats?.length) {
      return rows;
    }
    for (let row = 0; row < detail.rows; row++) {
      const start = row * detail.seatsPerRow;
      const end = start + detail.seatsPerRow;
      rows.push(detail.seats.slice(start, end));
    }
    return rows;
  }

  back(): void {
    this.location.back();
  }
}
