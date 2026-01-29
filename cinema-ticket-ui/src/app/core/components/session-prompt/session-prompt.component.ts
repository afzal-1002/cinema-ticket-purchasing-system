import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { SessionTimeoutService } from '../../services/session-timeout.service';

@Component({
  selector: 'app-session-prompt',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './session-prompt.component.html',
  styleUrls: ['./session-prompt.component.css']
})
export class SessionPromptComponent {
  constructor(private sessionTimeoutService: SessionTimeoutService) {}

  get state$() {
    return this.sessionTimeoutService.promptState$;
  }

  keepAlive(): void {
    this.sessionTimeoutService.continueSession();
  }

  endSession(): void {
    this.sessionTimeoutService.declineSession();
  }
}
