import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Movie, MoviePayload } from '../models/movie.model';

@Injectable({ providedIn: 'root' })
export class MovieService {
  private readonly baseUrl = '/api/movies';

  constructor(private http: HttpClient) {}

  getMovies(includeInactive = false): Observable<Movie[]> {
    if (includeInactive) {
      const params = new HttpParams().set('includeInactive', 'true');
      return this.http.get<Movie[]>(this.baseUrl, { params });
    }
    return this.http.get<Movie[]>(this.baseUrl);
  }

  getActiveMovies(): Observable<Movie[]> {
    return this.getMovies(false);
  }

  getMovie(id: number): Observable<Movie> {
    return this.http.get<Movie>(`${this.baseUrl}/${id}`);
  }

  createMovie(payload: MoviePayload): Observable<Movie> {
    return this.http.post<Movie>(this.baseUrl, payload);
  }

  updateMovie(id: number, payload: MoviePayload): Observable<Movie> {
    return this.http.put<Movie>(`${this.baseUrl}/${id}`, payload);
  }

  deleteMovie(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
