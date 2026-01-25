export interface CinemaSummary {
  id: number;
  name: string;
  rows: number;
  seatsPerRow: number;
  totalSeats?: number;
}

export interface CinemaPayload {
  name: string;
  rows: number;
  seatsPerRow: number;
}
