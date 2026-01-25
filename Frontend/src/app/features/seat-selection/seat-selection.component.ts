import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, Subscription, timer } from 'rxjs';
import { ReservationService } from '../../core/services/reservation.service';
import { ScreeningService } from '../../core/services/screening.service';
import { SeatSelectionService } from '../../core/services/seat-selection.service';
import {
  SeatHoldResponse,
  SeatMapResponse,
  SeatStatus,
  SeatSummary
} from '../../core/models/screening.model';
import { ReservationSummary } from '../../core/models/reservation.model';

interface SeatCoordinate {
  row: number;
  seat: number;
}

@Component({
  selector: 'app-seat-selection',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './seat-selection.component.html',
  styleUrls: ['./seat-selection.component.css']
})
export class SeatSelectionComponent implements OnInit, OnDestroy {
  seatRows: SeatSummary[][] = [];
  screeningInfo: SeatMapResponse | null = null;
  selectedSeats: SeatSummary[] = [];
  activeHold: SeatHoldResponse | null = null;
  myReservations: ReservationSummary[] = [];
  isLoading = false;
  isHolding = false;
  isConfirming = false;
  message: string | null = null;
  errorMessage: string | null = null;
  holdCountdownLabel = '';

  private screeningId!: number;
  private timerSub: Subscription | null = null;
  private dataSub: Subscription | null = null;
  private routeSub: Subscription | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private seatSelectionService: SeatSelectionService,
    private reservationService: ReservationService,
    private screeningService: ScreeningService
  ) {}

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!id) {
        this.errorMessage = 'Invalid screening id';
        return;
      }
      this.screeningId = id;
      this.loadAll();
    });
  }

  ngOnDestroy(): void {
    this.timerSub?.unsubscribe();
    this.dataSub?.unsubscribe();
    this.routeSub?.unsubscribe();
  }

  loadAll(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.dataSub?.unsubscribe();
    this.selectedSeats = [];

    const seatMap$ = this.seatSelectionService.getSeatMap(this.screeningId);
    const reservations$ = this.reservationService.getMyReservations();

    this.dataSub = forkJoin([seatMap$, reservations$]).subscribe({
      next: ([seatMap, reservations]) => {
        this.screeningInfo = seatMap;
        this.myReservations = reservations.filter(
          (res) => res.screeningId === this.screeningId
        );
        this.computeSeatRows(seatMap);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load seat map right now.';
        this.isLoading = false;
      }
    });
  }

  private computeSeatRows(map: SeatMapResponse): void {
    if (!map.seats?.length) {
      this.seatRows = [];
      return;
    }

    const myReservationKeys = new Set(
      this.myReservations.map((res) => `${res.row}-${res.seat}`)
    );

    const rows = new Map<number, SeatSummary[]>();
    map.seats.forEach((seat) => {
      const key = `${seat.row}-${seat.seat}`;
      const status: SeatStatus = myReservationKeys.has(key)
        ? 'MY_RESERVED'
        : (seat.status as SeatStatus);
      const decoratedSeat = { ...seat, status };
      const currentRow = rows.get(seat.row) ?? [];
      currentRow.push(decoratedSeat);
      rows.set(seat.row, currentRow);
    });

    this.seatRows = Array.from(rows.entries())
      .sort((a, b) => a[0] - b[0])
      .map(([, seatList]) => seatList.sort((a, b) => a.seat - b.seat));
  }

  toggleSeat(seat: SeatSummary): void {
    if (seat.status !== 'AVAILABLE') {
      return;
    }

    const exists = this.selectedSeats.find(
      (selected) => selected.row === seat.row && selected.seat === seat.seat
    );

    if (exists) {
      this.selectedSeats = this.selectedSeats.filter(
        (selected) => !(selected.row === seat.row && selected.seat === seat.seat)
      );
    } else {
      this.selectedSeats = [...this.selectedSeats, seat];
    }
  }

  holdSelectedSeats(): void {
    if (!this.selectedSeats.length || this.activeHold) {
      if (this.activeHold) {
        this.errorMessage = 'Release your current hold before creating another.';
      }
      return;
    }
    this.isHolding = true;
    this.errorMessage = null;
    this.seatSelectionService
      .holdSeats(
        this.screeningId,
        this.selectedSeats.map((seat) => ({ row: seat.row, seat: seat.seat }))
      )
      .subscribe({
        next: (hold) => {
          this.activeHold = hold;
          this.selectedSeats = [];
          this.message = `Seats held for ${hold.expiresInMinutes} minutes.`;
          this.startHoldCountdown(hold.expiresAt);
          this.loadAll();
          this.isHolding = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Unable to hold seats.';
          this.isHolding = false;
        }
      });
  }

  confirmReservation(): void {
    if (!this.activeHold) {
      return;
    }
    this.isConfirming = true;
    const requests = this.activeHold.seats.map((seat) =>
      this.reservationService.reserveSeat({
        screeningId: this.screeningId,
        row: seat.row,
        seat: seat.seat
      })
    );

    forkJoin(requests).subscribe({
      next: () => {
        const holdIds = this.activeHold?.holdIds ?? [];
        this.activeHold = null;
        this.stopCountdown();
        this.message = 'Reservation confirmed!';
        this.isConfirming = false;
        if (holdIds.length) {
          this.seatSelectionService.releaseHolds(holdIds).subscribe({
            next: () => {},
            error: () => {}
          });
        }
        this.loadAll();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Reservation failed.';
        this.isConfirming = false;
      }
    });
  }

  releaseHold(): void {
    if (!this.activeHold) {
      return;
    }
    const holdIds = this.activeHold.holdIds;
    this.seatSelectionService.releaseHolds(holdIds).subscribe({
      next: () => {
        this.activeHold = null;
        this.stopCountdown();
        this.message = 'Hold released.';
        this.loadAll();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Unable to release hold.';
      }
    });
  }

  cancelReservation(reservation: ReservationSummary): void {
    this.reservationService
      .cancelReservation(reservation.screeningId, reservation.row, reservation.seat)
      .subscribe({
        next: () => {
          this.message = 'Reservation cancelled.';
          this.loadAll();
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Unable to cancel seat.';
        }
      });
  }

  seatClass(seat: SeatSummary): string {
    const status = seat.status?.toLowerCase() ?? 'available';
    return `seat ${status} ${this.isSelected(seat) ? 'selected' : ''}`.trim();
  }

  isSelected(seat: SeatSummary): boolean {
    return this.selectedSeats.some(
      (selected) => selected.row === seat.row && selected.seat === seat.seat
    );
  }

  private startHoldCountdown(expiresAt: string): void {
    this.stopCountdown();
    const expiry = new Date(expiresAt).getTime();
    this.timerSub = timer(0, 1000).subscribe(() => {
      const diff = expiry - Date.now();
      if (diff <= 0) {
        this.holdCountdownLabel = 'expired';
        this.stopCountdown();
        return;
      }
      const minutes = Math.floor(diff / 60000);
      const seconds = Math.floor((diff % 60000) / 1000);
      this.holdCountdownLabel = `${minutes}m ${seconds}s remaining`;
    });
  }

  private stopCountdown(): void {
    this.timerSub?.unsubscribe();
    this.timerSub = null;
    this.holdCountdownLabel = '';
  }

  goBack(): void {
    this.router.navigate(['/browse/screenings']);
  }
}
