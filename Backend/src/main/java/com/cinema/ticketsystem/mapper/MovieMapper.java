package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.MovieDTO;
import com.cinema.ticketsystem.model.Movie;
import org.springframework.stereotype.Component;

@Component
public class MovieMapper {
    
    public MovieDTO toDTO(Movie movie) {
        if (movie == null) {
            return null;
        }
        
        MovieDTO dto = new MovieDTO();
        dto.setId(movie.getId());
        dto.setTitle(movie.getTitle());
        dto.setDescription(movie.getDescription());
        dto.setDurationMinutes(movie.getDurationMinutes());
        dto.setGenre(movie.getGenre());
        dto.setRating(movie.getRating());
        dto.setDirector(movie.getDirector());
        dto.setCast(movie.getCast());
        dto.setPosterUrl(movie.getPosterUrl());
        dto.setTrailerUrl(movie.getTrailerUrl());
        dto.setReleaseDate(movie.getReleaseDate());
        dto.setAverageRating(movie.getAverageRating());
        dto.setTotalReviews(movie.getTotalReviews());
        dto.setIsActive(movie.getIsActive());
        dto.setCreatedAt(movie.getCreatedAt());
        dto.setUpdatedAt(movie.getUpdatedAt());
        return dto;
    }
}
