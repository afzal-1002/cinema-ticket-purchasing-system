import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CinemaService } from '../../../core/services/cinema.service';
import { CinemaPayload, CinemaSummary } from '../../../core/models/cinema.model';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-cinema-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './cinema-list.component.html',
  styleUrls: ['./cinema-list.component.css']
})
export class CinemaListComponent implements OnInit {
  cinemas: CinemaSummary[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  form: FormGroup;
  editorVisible = false;
  editorMode: 'create' | 'edit' = 'create';
  activeCinema: CinemaSummary | null = null;
  formError: string | null = null;
  isSaving = false;
  deletingId: number | null = null;
  isAdmin = false;

  constructor(
    private cinemaService: CinemaService,
    private location: Location,
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      rows: [1, [Validators.required, Validators.min(1)]],
      seatsPerRow: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    this.fetchCinemas();
  }

  fetchCinemas(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cinemaService.getCinemas().subscribe({
      next: (cinemas) => {
        this.cinemas = cinemas;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load cinemas. Please try again shortly.';
        this.isLoading = false;
      }
    });
  }

  totalSeats(cinema: CinemaSummary): number {
    if (cinema.totalSeats != null) {
      return cinema.totalSeats;
    }
    return (cinema.rows || 0) * (cinema.seatsPerRow || 0);
  }

  buildArray(count: number): number[] {
    const safeCount = Math.max(count ?? 0, 0);
    return Array.from({ length: safeCount });
  }

  trackById(_index: number, cinema: CinemaSummary): number {
    return cinema.id;
  }

  goBack(): void {
    this.location.back();
  }

  startCreate(): void {
    if (!this.isAdmin) {
      return;
    }
    this.editorMode = 'create';
    this.activeCinema = null;
    this.formError = null;
    this.editorVisible = true;
    this.form.reset({ name: '', rows: 1, seatsPerRow: 1 });
  }

  startEdit(cinema: CinemaSummary): void {
    if (!this.isAdmin) {
      return;
    }
    this.editorMode = 'edit';
    this.activeCinema = cinema;
    this.formError = null;
    this.editorVisible = true;
    this.form.reset({
      name: cinema.name,
      rows: cinema.rows,
      seatsPerRow: cinema.seatsPerRow
    });
  }

  cancelEditor(): void {
    if (!this.isAdmin) {
      return;
    }
    this.editorVisible = false;
    this.activeCinema = null;
    this.formError = null;
    this.isSaving = false;
    this.form.reset({ name: '', rows: 1, seatsPerRow: 1 });
    this.form.markAsPristine();
    this.form.markAsUntouched();
  }

  submitEditor(): void {
    if (!this.isAdmin) {
      return;
    }
    if (this.form.invalid || this.isSaving) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.value as CinemaPayload;
    this.isSaving = true;
    this.formError = null;

    const request$ = this.activeCinema
      ? this.cinemaService.updateCinema(this.activeCinema.id, payload)
      : this.cinemaService.createCinema(payload);

    request$.subscribe({
      next: () => {
        this.isSaving = false;
        this.fetchCinemas();
        this.cancelEditor();
      },
      error: () => {
        this.isSaving = false;
        this.formError = 'Failed to save cinema. Please try again.';
      }
    });
  }

  deleteCinema(cinema: CinemaSummary): void {
    if (!this.isAdmin) {
      return;
    }
    const confirmed = confirm(
      `Delete ${cinema.name}? This removes all screenings and bookings for this cinema.`
    );
    if (!confirmed) {
      return;
    }

    this.deletingId = cinema.id;
    this.formError = null;

    this.cinemaService.deleteCinema(cinema.id).subscribe({
      next: () => {
        this.deletingId = null;
        if (this.activeCinema?.id === cinema.id) {
          this.cancelEditor();
        }
        this.fetchCinemas();
      },
      error: () => {
        this.deletingId = null;
        this.formError = 'Failed to delete cinema. Please try again.';
      }
    });
  }
}
