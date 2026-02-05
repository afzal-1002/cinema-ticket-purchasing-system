package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.model.Payment;

import java.util.List;
import java.util.Map;

public interface PaymentService {
    
    Map<String, Object> initiatePayment(Long userId, Long reservationId, Payment.PaymentMethod paymentMethod);
    
    Map<String, Object> processPayment(Long paymentId, Map<String, String> paymentDetails);
    
    Map<String, Object> refundPayment(Long paymentId, String reason);
    
    List<Map<String, Object>> getUserPayments(Long userId);
    
    Map<String, Object> getPaymentById(Long paymentId);
    
    Map<String, Object> getPaymentByReservation(Long reservationId);
}
