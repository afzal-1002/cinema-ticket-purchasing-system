import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthResponse, LoginPayload, RegisterPayload, UserSummary } from '../models/auth.model';

const TOKEN_KEY = 'jwt';
const USER_KEY = 'authUser';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly loginUrl = '/api/auth/login';

  constructor(private http: HttpClient) {}

  login(payload: LoginPayload): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(this.loginUrl, payload);
  }

  register(payload: RegisterPayload): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', payload);
  }

  storeSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    if (response.user) {
      localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    }
  }

  clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  getStoredUser(): UserSummary | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as UserSummary;
    } catch (error) {
      console.warn('Failed to parse stored user', error);
      return null;
    }
  }

  isAdmin(): boolean {
    return !!this.getStoredUser()?.isAdmin;
  }
}
