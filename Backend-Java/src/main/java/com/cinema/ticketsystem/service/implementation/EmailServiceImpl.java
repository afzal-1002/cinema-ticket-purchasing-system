package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.model.Reservation;
import com.cinema.ticketsystem.model.User;
import com.cinema.ticketsystem.service.EmailService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.mail.SimpleMailMessage;
import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.mail.javamail.MimeMessageHelper;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Service;

import jakarta.mail.MessagingException;
import jakarta.mail.internet.MimeMessage;
import java.time.format.DateTimeFormatter;

@Service
@RequiredArgsConstructor
@Slf4j
public class EmailServiceImpl implements EmailService {
    
    private final JavaMailSender mailSender;
    
    @Value("${spring.mail.username:noreply@cinema.com}")
    private String fromEmail;
    
    @Value("${app.name:Cinema Ticket System}")
    private String appName;
    
    @Async
    public void sendBookingConfirmation(User user, Reservation reservation) {
        try {
            String userEmail = user.getEmail();
            if (userEmail == null || userEmail.isEmpty()) {
                log.warn("Cannot send booking confirmation: user email is null or empty");
                return;
            }
            
            MimeMessage message = mailSender.createMimeMessage();
            MimeMessageHelper helper = new MimeMessageHelper(message, true, "UTF-8");
            
            helper.setFrom(fromEmail != null ? fromEmail : "noreply@cinema.com");
            helper.setTo(userEmail);
            helper.setSubject("Booking Confirmation - " + reservation.getScreening().getMovie().getTitle());
            
            String htmlContent = buildBookingConfirmationHtml(user, reservation);
            if (htmlContent != null) {
                helper.setText(htmlContent, true);
            }
            
            mailSender.send(message);
            log.info("Booking confirmation email sent to: {}", user.getEmail());
            
        } catch (MessagingException e) {
            log.error("Failed to send booking confirmation email to: {}", user.getEmail(), e);
        }
    }
    
    @Async
    public void sendBookingReminder(User user, Reservation reservation) {
        try {
            String userEmail = user.getEmail();
            if (userEmail == null || userEmail.isEmpty()) {
                log.warn("Cannot send booking reminder: user email is null or empty");
                return;
            }
            
            MimeMessage message = mailSender.createMimeMessage();
            MimeMessageHelper helper = new MimeMessageHelper(message, true, "UTF-8");
            
            helper.setFrom(fromEmail != null ? fromEmail : "noreply@cinema.com");
            helper.setTo(userEmail);
            helper.setSubject("Reminder: Your Movie Starts Soon - " + reservation.getScreening().getMovie().getTitle());
            
            String htmlContent = buildBookingReminderHtml(user, reservation);
            if (htmlContent != null) {
                helper.setText(htmlContent, true);
            }
            
            mailSender.send(message);
            log.info("Booking reminder email sent to: {}", user.getEmail());
            
        } catch (MessagingException e) {
            log.error("Failed to send booking reminder email to: {}", user.getEmail(), e);
        }
    }
    
    @Async
    public void sendPromotionalEmail(User user, String subject, String content) {
        try {
            String userEmail = user.getEmail();
            if (userEmail == null || userEmail.isEmpty()) {
                log.warn("Cannot send promotional email: user email is null or empty");
                return;
            }
            
            MimeMessage message = mailSender.createMimeMessage();
            MimeMessageHelper helper = new MimeMessageHelper(message, true, "UTF-8");
            
            helper.setFrom(fromEmail != null ? fromEmail : "noreply@cinema.com");
            helper.setTo(userEmail);
            String emailSubject = subject != null ? subject : "Promotional Email - " + appName;
            helper.setSubject(emailSubject);
            
            String htmlContent = buildPromotionalEmailHtml(user, emailSubject, content);
            if (htmlContent != null) {
                helper.setText(htmlContent, true);
            }
            
            mailSender.send(message);
            log.info("Promotional email sent to: {}", user.getEmail());
            
        } catch (MessagingException e) {
            log.error("Failed to send promotional email to: {}", user.getEmail(), e);
        }
    }
    
    @Async
    public void sendPasswordResetEmail(User user, String resetToken) {
        try {
            SimpleMailMessage message = new SimpleMailMessage();
            message.setFrom(fromEmail != null ? fromEmail : "noreply@cinema.com");
            message.setTo(user.getEmail());
            message.setSubject("Password Reset Request - " + appName);
            message.setText("Dear " + user.getFirstName() + ",\n\n" +
                    "You have requested to reset your password.\n\n" +
                    "Your password reset token is: " + resetToken + "\n\n" +
                    "This token will expire in 24 hours.\n\n" +
                    "If you did not request this, please ignore this email.\n\n" +
                    "Best regards,\n" + appName);
            
            mailSender.send(message);
            log.info("Password reset email sent to: {}", user.getEmail());
            
        } catch (Exception e) {
            log.error("Failed to send password reset email to: {}", user.getEmail(), e);
        }
    }
    
    @Async
    public void sendWelcomeEmail(User user) {
        try {
            String userEmail = user.getEmail();
            if (userEmail == null || userEmail.isEmpty()) {
                log.warn("Cannot send welcome email: user email is null or empty");
                return;
            }
            
            MimeMessage message = mailSender.createMimeMessage();
            MimeMessageHelper helper = new MimeMessageHelper(message, true, "UTF-8");
            
            helper.setFrom(fromEmail != null ? fromEmail : "noreply@cinema.com");
            helper.setTo(userEmail);
            helper.setSubject("Welcome to " + appName);
            
            String htmlContent = buildWelcomeEmailHtml(user);
            if (htmlContent != null) {
                helper.setText(htmlContent, true);
            }
            
            mailSender.send(message);
            log.info("Welcome email sent to: {}", user.getEmail());
            
        } catch (MessagingException e) {
            log.error("Failed to send welcome email to: {}", user.getEmail(), e);
        }
    }
    
    private String buildBookingConfirmationHtml(User user, Reservation reservation) {
        DateTimeFormatter dateFormatter = DateTimeFormatter.ofPattern("EEEE, MMMM dd, yyyy");
        DateTimeFormatter timeFormatter = DateTimeFormatter.ofPattern("hh:mm a");
        
        return """
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                    .header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; background-color: #f9f9f9; }
                    .ticket-info { background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #4CAF50; }
                    .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Booking Confirmed!</h1>
                    </div>
                    <div class="content">
                        <p>Dear %s,</p>
                        <p>Your movie ticket has been successfully booked!</p>
                        
                        <div class="ticket-info">
                            <h3>Booking Details</h3>
                            <p><strong>Movie:</strong> %s</p>
                            <p><strong>Date:</strong> %s</p>
                            <p><strong>Time:</strong> %s</p>
                            <p><strong>Cinema:</strong> %s</p>
                            <p><strong>Seat:</strong> Row %d, Seat %d</p>
                            <p><strong>Booking Reference:</strong> #%d</p>
                        </div>
                        
                        <p>Please arrive at least 15 minutes before the showtime.</p>
                        <p>We look forward to seeing you!</p>
                    </div>
                    <div class="footer">
                        <p>This is an automated email. Please do not reply.</p>
                        <p>&copy; 2026 %s. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
            """.formatted(
                user.getFirstName(),
                reservation.getScreening().getMovie().getTitle(),
                reservation.getScreening().getStartDateTime().format(dateFormatter),
                reservation.getScreening().getStartDateTime().format(timeFormatter),
                reservation.getScreening().getCinema().getName(),
                reservation.getRow(),
                reservation.getSeat(),
                reservation.getId(),
                appName
            );
    }
    
    private String buildBookingReminderHtml(User user, Reservation reservation) {
        DateTimeFormatter timeFormatter = DateTimeFormatter.ofPattern("hh:mm a");
        
        return """
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                    .header { background-color: #FF9800; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; background-color: #f9f9f9; }
                    .reminder { background-color: #FFF3CD; padding: 15px; margin: 15px 0; border-left: 4px solid #FF9800; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Showtime Reminder</h1>
                    </div>
                    <div class="content">
                        <p>Dear %s,</p>
                        <p>Your movie starts soon!</p>
                        
                        <div class="reminder">
                            <h3>%s</h3>
                            <p><strong>Showtime:</strong> %s</p>
                            <p><strong>Cinema:</strong> %s</p>
                            <p><strong>Your Seat:</strong> Row %d, Seat %d</p>
                        </div>
                        
                        <p>Don't forget to arrive early. Enjoy the show!</p>
                    </div>
                </div>
            </body>
            </html>
            """.formatted(
                user.getFirstName(),
                reservation.getScreening().getMovie().getTitle(),
                reservation.getScreening().getStartDateTime().format(timeFormatter),
                reservation.getScreening().getCinema().getName(),
                reservation.getRow(),
                reservation.getSeat()
            );
    }
    
    private String buildPromotionalEmailHtml(User user, String subject, String content) {
        return """
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                    .header { background-color: #2196F3; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; background-color: #f9f9f9; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>%s</h1>
                    </div>
                    <div class="content">
                        <p>Dear %s,</p>
                        %s
                    </div>
                </div>
            </body>
            </html>
            """.formatted(subject, user.getFirstName(), content);
    }
    
    private String buildWelcomeEmailHtml(User user) {
        return """
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                    .header { background-color: #9C27B0; color: white; padding: 20px; text-align: center; }
                    .content { padding: 20px; background-color: #f9f9f9; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>Welcome to %s!</h1>
                    </div>
                    <div class="content">
                        <p>Dear %s,</p>
                        <p>Thank you for joining %s! We're excited to have you on board.</p>
                        <p>You can now:</p>
                        <ul>
                            <li>Browse our latest movie listings</li>
                            <li>Book tickets in advance</li>
                            <li>Select your preferred seats</li>
                            <li>Access exclusive member benefits</li>
                        </ul>
                        <p>Start exploring and enjoy the show!</p>
                    </div>
                </div>
            </body>
            </html>
            """.formatted(appName, user.getFirstName(), appName);
    }
}
