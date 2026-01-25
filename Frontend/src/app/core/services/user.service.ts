import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile, UpdateUserPayload } from '../models/user.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = '/api/users';

  constructor(private http: HttpClient, private authService: AuthService) {}

  getCurrentUser(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/me`, {
      headers: this.buildHeaders()
    });
  }

  updateUser(payload: UpdateUserPayload): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.baseUrl}/${payload.id}`, payload, {
      headers: this.buildHeaders()
    });
  }

  private buildHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    if (!token) {
      return new HttpHeaders();
    }
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }
}
