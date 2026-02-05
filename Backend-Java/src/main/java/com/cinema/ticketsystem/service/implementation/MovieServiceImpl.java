package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.dto.CreateMovieRequest;
import com.cinema.ticketsystem.dto.CreateReviewRequest;
import com.cinema.ticketsystem.dto.MovieDTO;
import com.cinema.ticketsystem.dto.ReviewDTO;
import com.cinema.ticketsystem.mapper.MovieMapper;
import com.cinema.ticketsystem.mapper.ReviewMapper;
import com.cinema.ticketsystem.model.Movie;
import com.cinema.ticketsystem.model.Review;
import com.cinema.ticketsystem.model.User;
import com.cinema.ticketsystem.repository.MovieRepository;
import com.cinema.ticketsystem.repository.ReviewRepository;
import com.cinema.ticketsystem.repository.UserRepository;
import com.cinema.ticketsystem.service.MovieService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDate;
import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class MovieServiceImpl implements MovieService {
    
    private final MovieRepository movieRepository;
    private final ReviewRepository reviewRepository;
    private final UserRepository userRepository;
    private final MovieMapper movieMapper;
    private final ReviewMapper reviewMapper;
    
    @Transactional(readOnly = true)
    public List<MovieDTO> getAllActiveMovies() {
        return movieRepository.findByIsActiveTrue().stream()
                .map(movieMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public List<MovieDTO> getAllMovies() {
        return movieRepository.findAll().stream()
                .map(movieMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public MovieDTO getMovieById(Long id) {
        if (id == null) {
            throw new RuntimeException("Movie ID cannot be null");
        }
        Movie movie = movieRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Movie not found with id: " + id));
        return movieMapper.toDTO(movie);
    }
    
    @Transactional(readOnly = true)
    public List<MovieDTO> getMoviesByGenre(String genre) {
        return movieRepository.findByGenreAndIsActiveTrue(genre).stream()
                .map(movieMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public List<MovieDTO> searchMoviesByTitle(String title) {
        return movieRepository.findByTitleContainingIgnoreCaseAndIsActiveTrue(title).stream()
                .map(movieMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public List<MovieDTO> getNewReleases() {
        LocalDate thirtyDaysAgo = LocalDate.now().minusDays(30);
        return movieRepository.findByReleaseDateAfterAndIsActiveTrue(thirtyDaysAgo).stream()
                .map(movieMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public List<MovieDTO> getTopRatedMovies() {
        return movieRepository.findTopRatedMovies().stream()
                .limit(10)
                .map(movieMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public List<String> getAllGenres() {
        return movieRepository.findDistinctGenres();
    }
    
    @Transactional
    public MovieDTO createMovie(CreateMovieRequest request) {
        Movie movie = new Movie();
        movie.setTitle(request.getTitle());
        movie.setDescription(request.getDescription());
        movie.setDurationMinutes(request.getDurationMinutes());
        movie.setGenre(request.getGenre());
        movie.setRating(request.getRating());
        movie.setDirector(request.getDirector());
        movie.setCast(request.getCast());
        movie.setPosterUrl(request.getPosterUrl());
        movie.setTrailerUrl(request.getTrailerUrl());
        movie.setReleaseDate(request.getReleaseDate());
        movie.setIsActive(true);
        movie.setAverageRating(BigDecimal.ZERO);
        movie.setTotalReviews(0);
        
        Movie savedMovie = movieRepository.save(movie);
        return movieMapper.toDTO(savedMovie);
    }
    
    @Transactional
    public MovieDTO updateMovie(Long id, CreateMovieRequest request) {
        if (id == null) {
            throw new RuntimeException("Movie ID cannot be null");
        }
        Movie movie = movieRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Movie not found with id: " + id));
        
        if (request.getTitle() != null) movie.setTitle(request.getTitle());
        if (request.getDescription() != null) movie.setDescription(request.getDescription());
        if (request.getDurationMinutes() != null) movie.setDurationMinutes(request.getDurationMinutes());
        if (request.getGenre() != null) movie.setGenre(request.getGenre());
        if (request.getRating() != null) movie.setRating(request.getRating());
        if (request.getDirector() != null) movie.setDirector(request.getDirector());
        if (request.getCast() != null) movie.setCast(request.getCast());
        if (request.getPosterUrl() != null) movie.setPosterUrl(request.getPosterUrl());
        if (request.getTrailerUrl() != null) movie.setTrailerUrl(request.getTrailerUrl());
        if (request.getReleaseDate() != null) movie.setReleaseDate(request.getReleaseDate());    


        @SuppressWarnings("null")
        Movie updatedMovie = movieRepository.save(movie);
        return movieMapper.toDTO(updatedMovie);
    }
    
    @Transactional
    public void deactivateMovie(Long id) {
        if (id == null) {
            throw new RuntimeException("Movie ID cannot be null");
        }
        Movie movie = movieRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Movie not found with id: " + id));
        movie.setIsActive(false);
        movieRepository.save(movie);
    }
    
    @SuppressWarnings("null")
    @Transactional
    public void deleteMovie(Long id) {
        movieRepository.deleteById(id);
    }
    
    // Review Methods
    @Transactional
    public ReviewDTO createReview(Long userId, CreateReviewRequest request) {
        @SuppressWarnings("null")
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("User not found with id: " + userId));
        
        @SuppressWarnings("null")
        Movie movie = movieRepository.findById(request.getMovieId())
                .orElseThrow(() -> new RuntimeException("Movie not found with id: " + request.getMovieId()));
        
        if (reviewRepository.existsByUserIdAndMovieId(userId, request.getMovieId())) {
            throw new RuntimeException("You have already reviewed this movie");
        }
        
        Review review = new Review();
        review.setUser(user);
        review.setMovie(movie);
        review.setRating(request.getRating());
        review.setComment(request.getComment());
        
        Review savedReview = reviewRepository.save(review);
        
        // Update movie average rating
        updateMovieAverageRating(movie.getId());
        
        return reviewMapper.toDTO(savedReview);
    }
    
    @Transactional
    public ReviewDTO updateReview(Long userId, Long reviewId, CreateReviewRequest request) {
        @SuppressWarnings("null")
        Review review = reviewRepository.findById(reviewId)
                .orElseThrow(() -> new RuntimeException("Review not found with id: " + reviewId));
        
        if (!review.getUser().getId().equals(userId)) {
            throw new RuntimeException("You can only update your own reviews");
        }
        
        review.setRating(request.getRating());
        review.setComment(request.getComment());
        
        Review updatedReview = reviewRepository.save(review);
        
        // Update movie average rating
        updateMovieAverageRating(review.getMovie().getId());
        
        return reviewMapper.toDTO(updatedReview);
    }
    
    @Transactional
    public void deleteReview(Long userId, Long reviewId) {
        @SuppressWarnings("null")
        Review review = reviewRepository.findById(reviewId)
                .orElseThrow(() -> new RuntimeException("Review not found with id: " + reviewId));
        
        if (!review.getUser().getId().equals(userId)) {
            throw new RuntimeException("You can only delete your own reviews");
        }
        
        Long movieId = review.getMovie().getId();
        reviewRepository.delete(review);
        
        // Update movie average rating
        updateMovieAverageRating(movieId);
    }
    
    @Transactional(readOnly = true)
    public List<ReviewDTO> getMovieReviews(Long movieId) {
        return reviewRepository.findByMovieId(movieId).stream()
                .map(reviewMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public List<ReviewDTO> getUserReviews(Long userId) {
        return reviewRepository.findByUserId(userId).stream()
                .map(reviewMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    private void updateMovieAverageRating(Long movieId) {
        List<Review> reviews = reviewRepository.findByMovieId(movieId);
        @SuppressWarnings("null")
        Movie movie = movieRepository.findById(movieId)
                .orElseThrow(() -> new RuntimeException("Movie not found"));
        
        if (reviews.isEmpty()) {
            movie.setAverageRating(BigDecimal.ZERO);
            movie.setTotalReviews(0);
        } else {
            double average = reviews.stream()
                    .mapToInt(Review::getRating)
                    .average()
                    .orElse(0.0);
            
            movie.setAverageRating(BigDecimal.valueOf(average).setScale(1, RoundingMode.HALF_UP));
            movie.setTotalReviews(reviews.size());
        }
        
        movieRepository.save(movie);
    }
}
