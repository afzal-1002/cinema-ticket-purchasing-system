import { CommonModule, Location } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Movie, MoviePayload } from '../../../core/models/movie.model';
import { MovieService } from '../../../core/services/movie.service';

@Component({
  selector: 'app-movie-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './movie-management.component.html',
  styleUrls: ['./movie-management.component.css']
})
export class MovieManagementComponent implements OnInit {
  movies: Movie[] = [];
  movieForm: FormGroup;
  editingMovie: Movie | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  @ViewChild('movieListSection') movieListSection?: ElementRef<HTMLElement>;

  constructor(
    private fb: FormBuilder,
    private movieService: MovieService,
    private location: Location
  ) {
    this.movieForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(2000)],
      durationMinutes: [120, [Validators.required, Validators.min(1)]],
      genre: ['', [Validators.required, Validators.maxLength(100)]],
      rating: ['', Validators.maxLength(10)],
      director: ['', Validators.maxLength(100)],
      cast: ['', Validators.maxLength(500)],
      posterUrl: ['', Validators.maxLength(500)],
      trailerUrl: ['', Validators.maxLength(500)],
      releaseDate: ['', Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadMovies();
  }

  loadMovies(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.movieService.getMovies(true).subscribe({
      next: (movies) => {
        this.movies = movies;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load movies right now.';
        this.isLoading = false;
      }
    });
  }

  startCreate(): void {
    this.editingMovie = null;
    this.movieForm.reset({
      title: '',
      description: '',
      durationMinutes: 120,
      genre: '',
      rating: '',
      director: '',
      cast: '',
      posterUrl: '',
      trailerUrl: '',
      releaseDate: '',
      isActive: true
    });
    this.successMessage = null;
  }

  startEdit(movie: Movie): void {
    this.editingMovie = movie;
    this.movieForm.patchValue({
      title: movie.title,
      description: movie.description ?? '',
      durationMinutes: movie.durationMinutes ?? 120,
      genre: movie.genre ?? '',
      rating: movie.rating ?? '',
      director: movie.director ?? '',
      cast: movie.cast ?? '',
      posterUrl: movie.posterUrl ?? '',
      trailerUrl: movie.trailerUrl ?? '',
      releaseDate: movie.releaseDate ?? '',
      isActive: movie.isActive ?? true
    });
    this.successMessage = null;
  }

  saveMovie(): void {
    if (this.movieForm.invalid) {
      this.movieForm.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();
    this.isSaving = true;
    this.errorMessage = null;

    const request$ = this.editingMovie
      ? this.movieService.updateMovie(this.editingMovie.id, payload)
      : this.movieService.createMovie(payload);

    request$.subscribe({
      next: () => {
        this.isSaving = false;
        this.successMessage = this.editingMovie ? 'Movie updated successfully.' : 'Movie created successfully.';
        this.startCreate();
        this.loadMovies();
      },
      error: () => {
        this.isSaving = false;
        this.errorMessage = 'Unable to save movie. Please fix form errors and try again.';
      }
    });
  }

  deleteMovie(movie: Movie): void {
    this.errorMessage = null;
    this.movieService.deleteMovie(movie.id).subscribe({
      next: () => {
        if (this.editingMovie?.id === movie.id) {
          this.startCreate();
        }
        this.loadMovies();
      },
      error: () => {
        this.errorMessage = 'Unable to delete movie right now. Please try again.';
      }
    });
  }

  toggleActive(movie: Movie): void {
    const payload = this.payloadFromMovie(movie, { isActive: !movie.isActive });
    this.movieService.updateMovie(movie.id, payload).subscribe({
      next: () => this.loadMovies(),
      error: () => {
        this.errorMessage = 'Unable to update movie status. Please refresh and try again.';
      }
    });
  }

  trackById(_index: number, movie: Movie): number {
    return movie.id;
  }

  scrollToMovies(): void {
    this.movieListSection?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  deleteEditingMovie(): void {
    if (!this.editingMovie) {
      return;
    }
    this.deleteMovie(this.editingMovie);
  }

  private buildPayload(): MoviePayload {
    const value = this.movieForm.value;
    return {
      title: value.title,
      description: value.description || '',
      durationMinutes: Number(value.durationMinutes),
      genre: value.genre,
      rating: value.rating || '',
      director: value.director || '',
      cast: value.cast || '',
      posterUrl: value.posterUrl || '',
      trailerUrl: value.trailerUrl || '',
      releaseDate: value.releaseDate,
      isActive: value.isActive
    };
  }

  private payloadFromMovie(movie: Movie, overrides: Partial<MoviePayload> = {}): MoviePayload {
    return {
      title: movie.title,
      description: movie.description || '',
      durationMinutes: movie.durationMinutes ?? 120,
      genre: movie.genre || 'Unknown',
      rating: movie.rating || '',
      director: movie.director || '',
      cast: movie.cast || '',
      posterUrl: movie.posterUrl || '',
      trailerUrl: movie.trailerUrl || '',
      releaseDate: movie.releaseDate || new Date().toISOString().slice(0, 10),
      isActive: movie.isActive,
      ...overrides
    };
  }

  goBack(): void {
    this.location.back();
  }
}
