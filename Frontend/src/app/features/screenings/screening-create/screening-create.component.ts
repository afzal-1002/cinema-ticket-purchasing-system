import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CinemaService } from '../../../core/services/cinema.service';
import { MovieService } from '../../../core/services/movie.service';
import { ScreeningService } from '../../../core/services/screening.service';
import { CinemaSummary } from '../../../core/models/cinema.model';
import { Movie } from '../../../core/models/movie.model';

@Component({
  selector: 'app-screening-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './screening-create.component.html',
  styleUrls: ['./screening-create.component.css']
})
export class ScreeningCreateComponent implements OnInit {
  form: FormGroup;
  cinemas: CinemaSummary[] = [];
  movies: Movie[] = [];
  isSaving = false;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  private cinemasLoaded = false;
  private moviesLoaded = false;

  constructor(
    private fb: FormBuilder,
    private cinemaService: CinemaService,
    private movieService: MovieService,
    private screeningService: ScreeningService,
    private router: Router,
    private location: Location
  ) {
    this.form = this.fb.group({
      cinemaId: [null, Validators.required],
      movieId: [null, Validators.required],
      startDateTime: ['', Validators.required],
      ticketPrice: [null, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.loadContext();
  }

  loadContext(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.cinemaService.getCinemas().subscribe({
      next: (cinemas) => {
        this.cinemas = cinemas;
        this.cinemasLoaded = true;
        this.checkReady();
      },
      error: () => {
        this.errorMessage = 'Failed to load cinemas list.';
        this.isLoading = false;
        this.cinemasLoaded = true;
      }
    });

    this.movieService.getActiveMovies().subscribe({
      next: (movies) => {
        this.movies = movies;
        this.moviesLoaded = true;
        this.checkReady();
      },
      error: () => {
        this.errorMessage = 'Failed to load movies list.';
        this.isLoading = false;
        this.moviesLoaded = true;
      }
    });
  }

  private checkReady(): void {
    if (this.cinemasLoaded && this.moviesLoaded) {
      this.isLoading = false;
    }
  }

  submit(): void {
    if (this.form.invalid || this.isSaving) {
      this.form.markAllAsTouched();
      return;
    }

    const { cinemaId, movieId, startDateTime, ticketPrice } = this.form.value;
    const payload = {
      cinemaId,
      movieId,
      startDateTime: new Date(startDateTime).toISOString(),
      ticketPrice
    };

    this.isSaving = true;
    this.errorMessage = null;
    this.successMessage = null;

    this.screeningService.createScreening(payload).subscribe({
      next: () => {
        this.isSaving = false;
        this.successMessage = 'Screening created successfully';
        this.router.navigate(['/screenings']);
      },
      error: () => {
        this.isSaving = false;
        this.errorMessage = 'Failed to create screening. Please verify data and try again.';
      }
    });
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/admin/welcome']);
  }
}
