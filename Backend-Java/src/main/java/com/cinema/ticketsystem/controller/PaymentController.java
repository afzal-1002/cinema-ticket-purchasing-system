package com.cinema.ticketsystem.controller;

import com.cinema.ticketsystem.model.Payment;
import com.cinema.ticketsystem.security.UserDetailsImpl;
import com.cinema.ticketsystem.service.PaymentService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/payments")
@RequiredArgsConstructor
public class PaymentController {
    
    private final PaymentService paymentService;
    
    @PostMapping("/initiate")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<Map<String, Object>> initiatePayment(
            @AuthenticationPrincipal UserDetailsImpl userDetails,
            @RequestBody Map<String, Object> request) {
        Long reservationId = Long.valueOf(request.get("reservationId").toString());
        Payment.PaymentMethod paymentMethod = Payment.PaymentMethod.valueOf(
                request.get("paymentMethod").toString().toUpperCase());
        
        Map<String, Object> response = paymentService.initiatePayment(
                userDetails.getId(), reservationId, paymentMethod);
        
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }
    
    @PostMapping("/{paymentId}/process")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<Map<String, Object>> processPayment(
            @PathVariable Long paymentId,
            @RequestBody Map<String, String> paymentDetails) {
        Map<String, Object> response = paymentService.processPayment(paymentId, paymentDetails);
        return ResponseEntity.ok(response);
    }
    
    @PostMapping("/{paymentId}/refund")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<Map<String, Object>> refundPayment(
            @PathVariable Long paymentId,
            @RequestBody Map<String, String> request) {
        String reason = request.getOrDefault("reason", "Refund requested");
        Map<String, Object> response = paymentService.refundPayment(paymentId, reason);
        return ResponseEntity.ok(response);
    }
    
    @GetMapping("/my-payments")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<List<Map<String, Object>>> getMyPayments(
            @AuthenticationPrincipal UserDetailsImpl userDetails) {
        return ResponseEntity.ok(paymentService.getUserPayments(userDetails.getId()));
    }
    
    @GetMapping("/{paymentId}")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<Map<String, Object>> getPaymentById(@PathVariable Long paymentId) {
        return ResponseEntity.ok(paymentService.getPaymentById(paymentId));
    }
    
    @GetMapping("/reservation/{reservationId}")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<Map<String, Object>> getPaymentByReservation(@PathVariable Long reservationId) {
        return ResponseEntity.ok(paymentService.getPaymentByReservation(reservationId));
    }
}
