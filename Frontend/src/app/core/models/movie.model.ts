export interface MovieSummary {
  id: number;
  title: string;
  genre?: string;
  durationMinutes?: number;
  releaseDate?: string;
  isActive?: boolean;
}

export interface Movie extends MovieSummary {
  description?: string;
  rating?: string;
  director?: string;
  cast?: string;
  posterUrl?: string;
  trailerUrl?: string;
  averageRating?: number;
  totalReviews?: number;
}

export interface MoviePayload {
  title: string;
  description?: string;
  durationMinutes: number;
  genre: string;
  rating?: string;
  director?: string;
  cast?: string;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate: string;
  isActive?: boolean;
}
