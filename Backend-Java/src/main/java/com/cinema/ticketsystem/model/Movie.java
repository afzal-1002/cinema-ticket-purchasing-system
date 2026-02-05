package com.cinema.ticketsystem.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.*;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.CreationTimestamp;
import org.hibernate.annotations.UpdateTimestamp;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

@Entity
@Table(name = "movies")
@Data
@NoArgsConstructor
@AllArgsConstructor
public class Movie {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @NotBlank
    @Size(max = 200)
    @Column(nullable = false)
    private String title;
    
    @Size(max = 2000)
    @Column(length = 2000)
    private String description;
    
    @NotNull
    @Min(1)
    @Column(nullable = false)
    private Integer durationMinutes;
    
    @NotBlank
    @Size(max = 100)
    @Column(nullable = false)
    private String genre;
    
    @Size(max = 10)
    @Column(length = 10)
    private String rating; // e.g., "PG-13", "R", "G"
    
    @Size(max = 100)
    private String director;
    
    @Size(max = 500)
    private String cast;
    
    @Column(length = 500)
    private String posterUrl;
    
    @Column(length = 500)
    private String trailerUrl;
    
    @NotNull
    @Column(nullable = false)
    private LocalDate releaseDate;
    
    @DecimalMin("0.0")
    @DecimalMax("10.0")
    @Column(precision = 3, scale = 1)
    private BigDecimal averageRating;
    
    @Min(0)
    @Column
    private Integer totalReviews = 0;
    
    @Column(nullable = false)
    private Boolean isActive = true;
    
    @CreationTimestamp
    @Column(nullable = false, updatable = false)
    private LocalDateTime createdAt;
    
    @UpdateTimestamp
    @Column(nullable = false)
    private LocalDateTime updatedAt;
    
    @OneToMany(mappedBy = "movie", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<Screening> screenings = new ArrayList<>();
    
    @OneToMany(mappedBy = "movie", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<Review> reviews = new ArrayList<>();
}
