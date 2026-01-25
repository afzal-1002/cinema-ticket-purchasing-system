export interface ReservationSummary {
  id: number;
  screeningId: number;
  movieTitle: string;
  startDateTime: string;
  cinemaName: string;
  row: number;
  seat: number;
  createdAt: string;
  status?: string;
}

export interface CreateReservationPayload {
  screeningId: number;
  row: number;
  seat: number;
}
