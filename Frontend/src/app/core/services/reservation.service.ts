import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateReservationPayload, ReservationSummary } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly baseUrl = '/api/reservations';

  constructor(private http: HttpClient) {}

  getMyReservations(): Observable<ReservationSummary[]> {
    return this.http.get<ReservationSummary[]>(`${this.baseUrl}/my`);
  }

  reserveSeat(payload: CreateReservationPayload): Observable<ReservationSummary> {
    return this.http.post<ReservationSummary>(this.baseUrl, payload);
  }

  cancelReservation(screeningId: number, row: number, seat: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/screening/${screeningId}/row/${row}/seat/${seat}`);
  }
}
