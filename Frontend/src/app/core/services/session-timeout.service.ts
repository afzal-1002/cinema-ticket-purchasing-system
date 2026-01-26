import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';

export interface SessionPromptState {
  visible: boolean;
  countdown: number;
}

@Injectable({ providedIn: 'root' })
export class SessionTimeoutService {
  private readonly responseWindowMs = 60_000;
  private expiryTimer: ReturnType<typeof setTimeout> | null = null;
  private responseTimer: ReturnType<typeof setTimeout> | null = null;
  private countdownInterval: ReturnType<typeof setInterval> | null = null;

  private promptStateSubject = new BehaviorSubject<SessionPromptState>({
    visible: false,
    countdown: this.responseWindowMs / 1000
  });

  promptState$ = this.promptStateSubject.asObservable();

  constructor(private authService: AuthService, private router: Router) {}

  initialize(): void {
    if (this.authService.getToken()) {
      this.schedulePrompt();
    }
  }

  onSessionStart(): void {
    this.schedulePrompt();
  }

  continueSession(): void {
    this.authService.refreshToken().subscribe({
      next: (response) => {
        this.authService.storeSession(response);
        this.hidePrompt();
        this.schedulePrompt();
      },
      error: () => this.logout()
    });
  }

  declineSession(): void {
    this.logout();
  }

  private schedulePrompt(): void {
    this.clearTimers();
    const expiresAt = this.authService.getTokenExpiration();
    if (!expiresAt) {
      return;
    }
    const delay = Math.max(expiresAt - Date.now(), 0);
    this.expiryTimer = setTimeout(() => this.showPrompt(), delay);
  }

  private showPrompt(): void {
    if (!this.authService.getToken()) {
      return;
    }
    this.promptStateSubject.next({ visible: true, countdown: this.responseWindowMs / 1000 });
    this.startCountdown();
    this.responseTimer = setTimeout(() => this.logout(), this.responseWindowMs);
  }

  private hidePrompt(): void {
    this.clearCountdown();
    this.promptStateSubject.next({ visible: false, countdown: this.responseWindowMs / 1000 });
  }

  private startCountdown(): void {
    this.clearCountdown();
    let remaining = this.responseWindowMs / 1000;
    this.countdownInterval = setInterval(() => {
      remaining -= 1;
      if (remaining <= 0) {
        this.clearCountdown();
      }
      this.promptStateSubject.next({ visible: true, countdown: Math.max(remaining, 0) });
    }, 1000);
  }

  private clearTimers(): void {
    if (this.expiryTimer) {
      clearTimeout(this.expiryTimer);
      this.expiryTimer = null;
    }
    if (this.responseTimer) {
      clearTimeout(this.responseTimer);
      this.responseTimer = null;
    }
    this.clearCountdown();
  }

  private clearCountdown(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
      this.countdownInterval = null;
    }
  }

  private logout(): void {
    this.hidePrompt();
    this.clearTimers();
    this.authService.clearSession();
    this.router.navigate(['/login'], { queryParams: { session: 'expired' } });
  }
}
