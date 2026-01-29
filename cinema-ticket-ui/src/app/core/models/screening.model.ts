import { CinemaSummary } from './cinema.model';

export type SeatStatus = 'AVAILABLE' | 'HELD' | 'RESERVED' | 'MY_RESERVED';

export interface SeatSummary {
  row: number;
  seat: number;
  seatNumber?: string;
  status: SeatStatus;
  isReserved?: boolean;
  userId?: number;
}

export interface ScreeningSummary {
  id: number;
  cinemaId: number;
  cinemaName: string;
  movieId: number;
  movieTitle: string;
  startDateTime: string;
  ticketPrice: number;
  rows: number;
  seatsPerRow: number;
  totalSeats: number;
  reservedSeats: number;
  heldSeats: number;
  availableSeats: number;
}

export interface ScreeningDetail extends ScreeningSummary {
  seats: SeatSummary[];
}

export interface CreateScreeningPayload {
  cinemaId: number;
  movieId: number;
  startDateTime: string;
  ticketPrice: number;
}

export interface ScreeningContext {
  cinemas: CinemaSummary[];
}

export interface SeatMapResponse {
  screeningId: number;
  movieTitle: string;
  cinemaName: string;
  startDateTime: string;
  totalRows: number;
  seatsPerRow: number;
  totalSeats: number;
  availableSeats: number;
  seats: SeatSummary[];
}

export interface SeatHoldResponse {
  holdIds: number[];
  seats: { row: number; seat: number }[];
  expiresAt: string;
  expiresInMinutes: number;
}
