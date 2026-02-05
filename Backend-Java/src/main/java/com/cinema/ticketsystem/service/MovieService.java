package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.dto.CreateMovieRequest;
import com.cinema.ticketsystem.dto.CreateReviewRequest;
import com.cinema.ticketsystem.dto.MovieDTO;
import com.cinema.ticketsystem.dto.ReviewDTO;

import java.util.List;

public interface MovieService {
    
    List<MovieDTO> getAllActiveMovies();
    
    List<MovieDTO> getAllMovies();
    
    MovieDTO getMovieById(Long id);
    
    List<MovieDTO> getMoviesByGenre(String genre);
    
    List<MovieDTO> searchMoviesByTitle(String title);
    
    List<MovieDTO> getNewReleases();
    
    List<MovieDTO> getTopRatedMovies();
    
    List<String> getAllGenres();
    
    MovieDTO createMovie(CreateMovieRequest request);
    
    MovieDTO updateMovie(Long id, CreateMovieRequest request);
    
    void deactivateMovie(Long id);
    
    void deleteMovie(Long id);
    
    ReviewDTO createReview(Long userId, CreateReviewRequest request);
    
    ReviewDTO updateReview(Long userId, Long reviewId, CreateReviewRequest request);
    
    void deleteReview(Long userId, Long reviewId);
    
    List<ReviewDTO> getMovieReviews(Long movieId);
    
    List<ReviewDTO> getUserReviews(Long userId);
}
