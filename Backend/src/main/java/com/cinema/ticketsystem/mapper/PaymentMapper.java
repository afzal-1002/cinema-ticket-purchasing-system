package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.PaymentDTO;
import com.cinema.ticketsystem.model.Payment;
import org.springframework.stereotype.Component;

@Component
public class PaymentMapper {
    
    public PaymentDTO toDTO(Payment payment) {
        if (payment == null) {
            return null;
        }
        
        PaymentDTO dto = new PaymentDTO();
        dto.setId(payment.getId());
        dto.setUserId(payment.getUser() != null ? payment.getUser().getId() : null);
        dto.setReservationId(payment.getReservation() != null ? payment.getReservation().getId() : null);
        dto.setAmount(payment.getAmount());
        dto.setStatus(payment.getStatus() != null ? payment.getStatus().name() : null);
        dto.setPaymentMethod(payment.getPaymentMethod() != null ? payment.getPaymentMethod().name() : null);
        dto.setTransactionId(payment.getTransactionId());
        dto.setPaymentDetails(payment.getPaymentDetails());
        dto.setFailureReason(payment.getFailureReason());
        dto.setCreatedAt(payment.getCreatedAt());
        dto.setUpdatedAt(payment.getUpdatedAt());
        dto.setCompletedAt(payment.getCompletedAt());
        return dto;
    }
}
