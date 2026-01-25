import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CinemaPayload, CinemaSummary } from '../models/cinema.model';

@Injectable({ providedIn: 'root' })
export class CinemaService {
  private readonly baseUrl = '/api/cinemas';

  constructor(private http: HttpClient) {}

  getCinemas(): Observable<CinemaSummary[]> {
    return this.http.get<CinemaSummary[]>(this.baseUrl);
  }

  createCinema(payload: CinemaPayload): Observable<CinemaSummary> {
    return this.http.post<CinemaSummary>(this.baseUrl, payload);
  }

  updateCinema(id: number, payload: CinemaPayload): Observable<CinemaSummary> {
    return this.http.put<CinemaSummary>(`${this.baseUrl}/${id}`, payload);
  }

  deleteCinema(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
