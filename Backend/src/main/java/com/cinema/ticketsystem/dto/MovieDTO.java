package com.cinema.ticketsystem.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class MovieDTO {
    private Long id;
    private String title;
    private String description;
    private Integer durationMinutes;
    private String genre;
    private String rating;
    private String director;
    private String cast;
    private String posterUrl;
    private String trailerUrl;
    private LocalDate releaseDate;
    private BigDecimal averageRating;
    private Integer totalReviews;
    private Boolean isActive;
    private LocalDateTime createdAt;
    private LocalDateTime updatedAt;
}
