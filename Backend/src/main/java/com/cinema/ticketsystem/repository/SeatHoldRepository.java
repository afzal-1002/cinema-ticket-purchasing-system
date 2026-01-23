package com.cinema.ticketsystem.repository;

import com.cinema.ticketsystem.model.SeatHold;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Repository
public interface SeatHoldRepository extends JpaRepository<SeatHold, Long> {
    
    List<SeatHold> findByScreeningIdAndIsActiveTrue(Long screeningId);
    
    List<SeatHold> findByUserIdAndIsActiveTrue(Long userId);
    
    Optional<SeatHold> findByScreeningIdAndRowAndSeatAndIsActiveTrue(
            Long screeningId, Integer row, Integer seat);
    
    boolean existsByScreeningIdAndRowAndSeatAndIsActiveTrue(
            Long screeningId, Integer row, Integer seat);
    
    @Modifying
    @Query("UPDATE SeatHold sh SET sh.isActive = false WHERE sh.expiresAt < :now AND sh.isActive = true")
    int deactivateExpiredHolds(LocalDateTime now);
    
    @Query("SELECT sh FROM SeatHold sh WHERE sh.expiresAt < :now AND sh.isActive = true")
    List<SeatHold> findExpiredHolds(LocalDateTime now);
}
