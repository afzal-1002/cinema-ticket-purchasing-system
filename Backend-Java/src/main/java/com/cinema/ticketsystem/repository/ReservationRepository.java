package com.cinema.ticketsystem.repository;

import com.cinema.ticketsystem.model.Reservation;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Repository
public interface ReservationRepository extends JpaRepository<Reservation, Long> {
    List<Reservation> findByUserIdOrderByScreening_StartDateTimeAsc(Long userId);
    List<Reservation> findByScreeningId(Long screeningId);
    Optional<Reservation> findByUserIdAndScreeningIdAndRowAndSeat(Long userId, Long screeningId, Integer row, Integer seat);
    Boolean existsByScreeningIdAndRowAndSeat(Long screeningId, Integer row, Integer seat);
    
    @Query("SELECT DATE(r.createdAt) as date, COUNT(r) as count FROM Reservation r " +
           "WHERE r.createdAt BETWEEN :startDate AND :endDate " +
           "GROUP BY DATE(r.createdAt) ORDER BY DATE(r.createdAt)")
    List<Object[]> findDailyBookingCount(LocalDateTime startDate, LocalDateTime endDate);
}
