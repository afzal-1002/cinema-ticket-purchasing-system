package com.cinema.ticketsystem.model;

import jakarta.persistence.*;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.hibernate.annotations.CreationTimestamp;

import java.time.LocalDateTime;

@Entity
@Table(name = "seat_holds", uniqueConstraints = {
    @UniqueConstraint(columnNames = {"screening_id", "row_number", "seat_number"})
})
@Data
@NoArgsConstructor
@AllArgsConstructor
public class SeatHold {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "user_id", nullable = false)
    private User user;
    
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "screening_id", nullable = false)
    private Screening screening;
    
    @NotNull
    @Min(1)
    @Column(name = "row_number", nullable = false)
    private Integer row;
    
    @NotNull
    @Min(1)
    @Column(name = "seat_number", nullable = false)
    private Integer seat;
    
    @CreationTimestamp
    @Column(nullable = false, updatable = false)
    private LocalDateTime heldAt;
    
    @NotNull
    @Column(nullable = false)
    private LocalDateTime expiresAt;
    
    @Column(nullable = false)
    private Boolean isActive = true;
}
