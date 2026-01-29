import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateScreeningPayload,
  ScreeningDetail,
  ScreeningSummary
} from '../models/screening.model';

@Injectable({ providedIn: 'root' })
export class ScreeningService {
  private readonly baseUrl = '/api/screenings';

  constructor(private http: HttpClient) {}

  getScreenings(): Observable<ScreeningSummary[]> {
    return this.http.get<ScreeningSummary[]>(this.baseUrl);
  }

  getScreeningById(id: number): Observable<ScreeningDetail> {
    return this.http.get<ScreeningDetail>(`${this.baseUrl}/${id}`);
  }

  createScreening(payload: CreateScreeningPayload): Observable<ScreeningSummary> {
    return this.http.post<ScreeningSummary>(this.baseUrl, payload);
  }

  deleteScreening(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
