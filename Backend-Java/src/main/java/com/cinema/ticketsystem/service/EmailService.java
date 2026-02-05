package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.model.Reservation;
import com.cinema.ticketsystem.model.User;

public interface EmailService {
    
    void sendBookingConfirmation(User user, Reservation reservation);
    
    void sendBookingReminder(User user, Reservation reservation);
    
    void sendPromotionalEmail(User user, String subject, String content);
    
    void sendPasswordResetEmail(User user, String resetToken);
    
    void sendWelcomeEmail(User user);
}
