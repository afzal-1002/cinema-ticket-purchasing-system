import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SessionPromptComponent } from './core/components/session-prompt/session-prompt.component';
import { SessionTimeoutService } from './core/services/session-timeout.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, SessionPromptComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'Frontend';

  constructor(private sessionTimeoutService: SessionTimeoutService) {
    this.sessionTimeoutService.initialize();
  }
}
