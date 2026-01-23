package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.model.*;
import com.cinema.ticketsystem.repository.*;
import com.cinema.ticketsystem.service.PaymentService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class PaymentServiceImpl implements PaymentService {
    
    private final PaymentRepository paymentRepository;
    private final ReservationRepository reservationRepository;
    private final UserRepository userRepository;
    
    @Transactional
    public Map<String, Object> initiatePayment(Long userId, Long reservationId, Payment.PaymentMethod paymentMethod) {
       if (userId==null) {
            throw new RuntimeException("User ID cannot be null");
       }
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        if (reservationId==null) {
            throw new RuntimeException("Reservation ID cannot be null");
        }
        Reservation reservation = reservationRepository.findById(reservationId)
                .orElseThrow(() -> new RuntimeException("Reservation not found"));
        
        if (!reservation.getUser().getId().equals(userId)) {
            throw new RuntimeException("You can only pay for your own reservations");
        }
        
        // Check if payment already exists
        Optional<Payment> existingPayment = paymentRepository.findByReservationId(reservationId);
        if (existingPayment.isPresent()) {
            Payment payment = existingPayment.get();
            if (payment.getStatus() == Payment.PaymentStatus.COMPLETED) {
                throw new RuntimeException("Payment already completed for this reservation");
            }
        }
        
        Screening screening = reservation.getScreening();
        BigDecimal amount = screening.getTicketPrice();
        
        Payment payment = new Payment();
        payment.setUser(user);
        payment.setReservation(reservation);
        payment.setAmount(amount);
        payment.setPaymentMethod(paymentMethod);
        payment.setStatus(Payment.PaymentStatus.PENDING);
        
        Payment savedPayment = paymentRepository.save(payment);
        
        Map<String, Object> response = new HashMap<>();
        response.put("paymentId", savedPayment.getId());
        response.put("amount", amount);
        response.put("currency", "USD");
        response.put("status", savedPayment.getStatus());
        response.put("paymentMethod", paymentMethod);
        response.put("reservationId", reservationId);
        
        // In a real implementation, here you would integrate with payment providers
        // like Stripe, PayPal, etc., and return their payment intent/session
        response.put("message", "Payment initiated. Use the processPayment endpoint to complete.");
        
        return response;
    }
    
    @Transactional
    public Map<String, Object> processPayment(Long paymentId, Map<String, String> paymentDetails) {
        
        if (paymentId == null) {
            throw new RuntimeException("Payment ID cannot be null");
        }
        Payment payment = paymentRepository.findById(paymentId)
                .orElseThrow(() -> new RuntimeException("Payment not found"));
        
        if (payment.getStatus() == Payment.PaymentStatus.COMPLETED) {
            throw new RuntimeException("Payment already completed");
        }
        
        payment.setStatus(Payment.PaymentStatus.PROCESSING);
        paymentRepository.save(payment);
        
        try {
            // Simulate payment processing
            // In real implementation, integrate with Stripe/PayPal API here
            String transactionId = "TXN-" + UUID.randomUUID().toString().substring(0, 8).toUpperCase();
            
            payment.setTransactionId(transactionId);
            payment.setStatus(Payment.PaymentStatus.COMPLETED);
            payment.setCompletedAt(LocalDateTime.now());
            payment.setPaymentDetails(convertMapToJson(paymentDetails));
            
            Payment completedPayment = paymentRepository.save(payment);
            
            Map<String, Object> response = new HashMap<>();
            response.put("paymentId", completedPayment.getId());
            response.put("transactionId", transactionId);
            response.put("status", "COMPLETED");
            response.put("amount", completedPayment.getAmount());
            response.put("completedAt", completedPayment.getCompletedAt());
            response.put("message", "Payment processed successfully");
            
            return response;
            
        } catch (Exception e) {
            payment.setStatus(Payment.PaymentStatus.FAILED);
            payment.setFailureReason(e.getMessage());
            paymentRepository.save(payment);
            
            throw new RuntimeException("Payment processing failed: " + e.getMessage());
        }
    }
    
    @Transactional
    public Map<String, Object> refundPayment(Long paymentId, String reason) {
        if (paymentId == null) {
            throw new RuntimeException("Payment ID cannot be null");
        }
        Payment payment = paymentRepository.findById(paymentId)
                .orElseThrow(() -> new RuntimeException("Payment not found"));
        
        if (payment.getStatus() != Payment.PaymentStatus.COMPLETED) {
            throw new RuntimeException("Can only refund completed payments");
        }
        
        // In real implementation, process refund through payment provider
        payment.setStatus(Payment.PaymentStatus.REFUNDED);
        payment.setFailureReason("Refund reason: " + reason);
        
        Payment refundedPayment = paymentRepository.save(payment);
        
        Map<String, Object> response = new HashMap<>();
        response.put("paymentId", refundedPayment.getId());
        response.put("status", "REFUNDED");
        response.put("refundAmount", refundedPayment.getAmount());
        response.put("refundReason", reason);
        response.put("message", "Payment refunded successfully");
        
        return response;
    }
    
    @Transactional(readOnly = true)
    public List<Map<String, Object>> getUserPayments(Long userId) {
        List<Payment> payments = paymentRepository.findByUserId(userId);
        return payments.stream()
                .map(this::convertPaymentToMap)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public Map<String, Object> getPaymentById(Long paymentId) {
        if (paymentId == null) {
            throw new RuntimeException("Payment ID cannot be null");
        }
        Payment payment = paymentRepository.findById(paymentId)
                .orElseThrow(() -> new RuntimeException("Payment not found"));
        return convertPaymentToMap(payment);
    }
    
    @Transactional(readOnly = true)
    public Map<String, Object> getPaymentByReservation(Long reservationId) {
        if (reservationId == null) {
            throw new RuntimeException("Reservation ID cannot be null");
        }
        Payment payment = paymentRepository.findByReservationId(reservationId)
                .orElseThrow(() -> new RuntimeException("No payment found for this reservation"));
        return convertPaymentToMap(payment);
    }
    
    private Map<String, Object> convertPaymentToMap(Payment payment) {
        Map<String, Object> map = new HashMap<>();
        map.put("id", payment.getId());
        map.put("amount", payment.getAmount());
        map.put("status", payment.getStatus());
        map.put("paymentMethod", payment.getPaymentMethod());
        map.put("transactionId", payment.getTransactionId());
        map.put("createdAt", payment.getCreatedAt());
        map.put("completedAt", payment.getCompletedAt());
        
        if (payment.getReservation() != null) {
            map.put("reservationId", payment.getReservation().getId());
            map.put("movieTitle", payment.getReservation().getScreening().getMovie().getTitle());
            map.put("screeningDateTime", payment.getReservation().getScreening().getStartDateTime());
            map.put("seat", String.format("Row %d, Seat %d", 
                payment.getReservation().getRow(), payment.getReservation().getSeat()));
        }
        
        if (payment.getFailureReason() != null) {
            map.put("failureReason", payment.getFailureReason());
        }
        
        return map;
    }
    
    private String convertMapToJson(Map<String, String> map) {
        // Simple JSON conversion - in production use Jackson ObjectMapper
        if (map == null || map.isEmpty()) return "{}";
        
        StringBuilder json = new StringBuilder("{");
        map.forEach((key, value) -> 
            json.append("\"").append(key).append("\":\"").append(value).append("\","));
        json.deleteCharAt(json.length() - 1); // Remove trailing comma
        json.append("}");
        
        return json.toString();
    }
}
