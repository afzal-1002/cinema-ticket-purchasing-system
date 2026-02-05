package com.cinema.ticketsystem.controller;

import com.cinema.ticketsystem.dto.CreateReservationRequest;
import com.cinema.ticketsystem.dto.ReservationDTO;
import com.cinema.ticketsystem.security.JwtTokenUtil;
import com.cinema.ticketsystem.service.ReservationService;
import jakarta.validation.Valid;
import lombok.Getter;
import lombok.Setter;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/reservations")
@CrossOrigin(origins = "http://localhost:4200")
public class ReservationController {
    
    @Autowired
    private ReservationService reservationService;
    
    @Autowired
    private JwtTokenUtil jwtTokenUtil;
    
    @GetMapping("/my")
    public ResponseEntity<?> getMyReservations(@RequestHeader("Authorization") String authHeader) {
        try {
            String token = authHeader.substring(7);
            Long userId = jwtTokenUtil.extractClaim(token, claims -> claims.get("userId", Long.class));
            
            List<ReservationDTO> reservations = reservationService.getUserReservations(userId);
            return ResponseEntity.ok(reservations);
        } catch (Exception e) {
            return ResponseEntity.badRequest().body(new ErrorResponse(e.getMessage()));
        }
    }
    
    @PostMapping
    public ResponseEntity<?> createReservation(@Valid @RequestBody CreateReservationRequest request,
                                              @RequestHeader("Authorization") String authHeader) {
        try {
            String token = authHeader.substring(7);
            Long userId = jwtTokenUtil.extractClaim(token, claims -> claims.get("userId", Long.class));
            
            ReservationDTO reservation = reservationService.createReservation(userId, request);
            return ResponseEntity.ok(reservation);
        } catch (IllegalArgumentException e) {
            return ResponseEntity.badRequest().body(new ErrorResponse(e.getMessage()));
        } catch (RuntimeException e) {
            if (e.getMessage().contains("already reserved")) {
                return ResponseEntity.status(409).body(new ErrorResponse(e.getMessage()));
            }
            return ResponseEntity.badRequest().body(new ErrorResponse(e.getMessage()));
        }
    }
    
    @DeleteMapping("/screening/{screeningId}/row/{row}/seat/{seat}")
    public ResponseEntity<?> deleteReservation(@PathVariable Long screeningId,
                                              @PathVariable Integer row,
                                              @PathVariable Integer seat,
                                              @RequestHeader("Authorization") String authHeader) {
        try {
            String token = authHeader.substring(7);
            Long userId = jwtTokenUtil.extractClaim(token, claims -> claims.get("userId", Long.class));
            
            reservationService.deleteReservation(userId, screeningId, row, seat);
            return ResponseEntity.noContent().build();
        } catch (Exception e) {
            return ResponseEntity.badRequest().body(new ErrorResponse(e.getMessage()));
        }
    }
    
    @Getter @Setter
    private static class ErrorResponse {
        public String message;
        public ErrorResponse(String message) {
            this.message = message;
        }
    }
}
