package com.cinema.ticketsystem.repository;

import com.cinema.ticketsystem.model.Payment;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Repository
public interface PaymentRepository extends JpaRepository<Payment, Long> {
    
    List<Payment> findByUserId(Long userId);
    
    Optional<Payment> findByReservationId(Long reservationId);
    
    List<Payment> findByStatus(Payment.PaymentStatus status);
    
    List<Payment> findByUserIdAndStatus(Long userId, Payment.PaymentStatus status);
    
    @Query("SELECT p FROM Payment p WHERE p.status = 'COMPLETED' AND p.completedAt BETWEEN :startDate AND :endDate")
    List<Payment> findCompletedPaymentsBetween(LocalDateTime startDate, LocalDateTime endDate);
    
    @Query("SELECT SUM(p.amount) FROM Payment p WHERE p.status = 'COMPLETED' AND p.completedAt BETWEEN :startDate AND :endDate")
    Optional<java.math.BigDecimal> getTotalRevenueBetween(LocalDateTime startDate, LocalDateTime endDate);
    
    Optional<Payment> findByTransactionId(String transactionId);
}
