package com.cinema.ticketsystem.dto;

import jakarta.validation.constraints.*;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDate;

// Create Movie Request DTO
@Data
@NoArgsConstructor
@AllArgsConstructor
public class CreateMovieRequest {
    
    @NotBlank(message = "Title is required")
    @Size(max = 200, message = "Title must not exceed 200 characters")
    private String title;
    
    @Size(max = 2000, message = "Description must not exceed 2000 characters")
    private String description;
    
    @NotNull(message = "Duration is required")
    @Min(value = 1, message = "Duration must be at least 1 minute")
    private Integer durationMinutes;
    
    @NotBlank(message = "Genre is required")
    @Size(max = 100, message = "Genre must not exceed 100 characters")
    private String genre;
    
    @Size(max = 10, message = "Rating must not exceed 10 characters")
    private String rating;
    
    @Size(max = 100, message = "Director name must not exceed 100 characters")
    private String director;
    
    @Size(max = 500, message = "Cast must not exceed 500 characters")
    private String cast;
    
    @Size(max = 500, message = "Poster URL must not exceed 500 characters")
    private String posterUrl;
    
    @Size(max = 500, message = "Trailer URL must not exceed 500 characters")
    private String trailerUrl;
    
    @NotNull(message = "Release date is required")
    private LocalDate releaseDate;
}

// Update Movie Request DTO
@Data
@NoArgsConstructor
@AllArgsConstructor
class UpdateMovieRequest {
    
    @Size(max = 200, message = "Title must not exceed 200 characters")
    private String title;
    
    @Size(max = 2000, message = "Description must not exceed 2000 characters")
    private String description;
    
    @Min(value = 1, message = "Duration must be at least 1 minute")
    private Integer durationMinutes;
    
    @Size(max = 100, message = "Genre must not exceed 100 characters")
    private String genre;
    
    @Size(max = 10, message = "Rating must not exceed 10 characters")
    private String rating;
    
    @Size(max = 100, message = "Director name must not exceed 100 characters")
    private String director;
    
    @Size(max = 500, message = "Cast must not exceed 500 characters")
    private String cast;
    
    @Size(max = 500, message = "Poster URL must not exceed 500 characters")
    private String posterUrl;
    
    @Size(max = 500, message = "Trailer URL must not exceed 500 characters")
    private String trailerUrl;
    
    private LocalDate releaseDate;
    
    private Boolean isActive;
}
