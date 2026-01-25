import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SeatHoldResponse, SeatMapResponse } from '../models/screening.model';

interface HoldRequestSeat {
  row: number;
  seat: number;
}

@Injectable({ providedIn: 'root' })
export class SeatSelectionService {
  private readonly baseUrl = '/api/seats';

  constructor(private http: HttpClient) {}

  getSeatMap(screeningId: number): Observable<SeatMapResponse> {
    return this.http.get<SeatMapResponse>(`${this.baseUrl}/screening/${screeningId}`);
  }

  holdSeats(screeningId: number, seats: HoldRequestSeat[]): Observable<SeatHoldResponse> {
    return this.http.post<SeatHoldResponse>(`${this.baseUrl}/hold`, {
      screeningId,
      seats
    });
  }

  releaseHolds(holdIds: number[]): Observable<void> {
    return this.http.request<void>('delete', `${this.baseUrl}/hold`, {
      body: { holdIds }
    });
  }

  releaseAll(): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/hold/all`);
  }

  getMyHolds(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/hold/my-holds`);
  }
}
