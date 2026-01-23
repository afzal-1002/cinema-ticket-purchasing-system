package com.cinema.ticketsystem.repository;

import com.cinema.ticketsystem.model.Movie;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.time.LocalDate;
import java.util.List;

@Repository
public interface MovieRepository extends JpaRepository<Movie, Long> {
    
    List<Movie> findByIsActiveTrue();
    
    List<Movie> findByGenre(String genre);
    
    List<Movie> findByGenreAndIsActiveTrue(String genre);
    
    List<Movie> findByTitleContainingIgnoreCaseAndIsActiveTrue(String title);
    
    List<Movie> findByReleaseDateAfterAndIsActiveTrue(LocalDate date);
    
    @Query("SELECT DISTINCT m.genre FROM Movie m WHERE m.isActive = true ORDER BY m.genre")
    List<String> findDistinctGenres();
    
    @Query("SELECT m FROM Movie m WHERE m.isActive = true ORDER BY m.averageRating DESC, m.totalReviews DESC")
    List<Movie> findTopRatedMovies();
    
    @Query("SELECT m FROM Movie m JOIN m.screenings s GROUP BY m.id ORDER BY COUNT(s) DESC")
    List<Movie> findMostScreenedMovies();
}
