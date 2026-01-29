import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthResponse, LoginPayload, RegisterPayload, UserSummary } from '../models/auth.model';

const TOKEN_KEY = 'jwt';
const USER_KEY = 'authUser';
const TOKEN_EXP_KEY = 'jwt_exp';

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

  refreshToken(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/refresh', {});
  }

  storeSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    const expiresAt = this.extractExpiration(response.token);
    if (expiresAt) {
      localStorage.setItem(TOKEN_EXP_KEY, expiresAt.toString());
    } else {
      localStorage.removeItem(TOKEN_EXP_KEY);
    }
    if (response.user) {
      localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    }
  }

  clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(TOKEN_EXP_KEY);
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

  updateStoredUser(user: UserSummary): void {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getTokenExpiration(): number | null {
    const stored = localStorage.getItem(TOKEN_EXP_KEY);
    if (stored) {
      const value = Number(stored);
      return Number.isNaN(value) ? null : value;
    }

    const token = this.getToken();
    const expiresAt = token ? this.extractExpiration(token) : null;
    if (expiresAt) {
      localStorage.setItem(TOKEN_EXP_KEY, expiresAt.toString());
    }
    return expiresAt;
  }

  isTokenExpired(): boolean {
    const expiresAt = this.getTokenExpiration();
    if (!expiresAt) {
      return true;
    }
    return Date.now() >= expiresAt;
  }

  hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }
    return !this.isTokenExpired();
  }

  private extractExpiration(token: string): number | null {
    try {
      const payload = this.decodePayload(token);
      if (payload?.exp) {
        return payload.exp * 1000;
      }
      return null;
    } catch {
      return null;
    }
  }

  private decodePayload(token: string): any | null {
    try {
      const base64Payload = token.split('.')[1];
      const decoded = atob(base64Payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch (error) {
      console.warn('Failed to decode token payload', error);
      return null;
    }
  }
}
