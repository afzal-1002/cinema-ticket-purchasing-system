package com.cinema.ticketsystem.controller;

import com.cinema.ticketsystem.dto.CreateScreeningRequest;
import com.cinema.ticketsystem.dto.ScreeningDTO;
import com.cinema.ticketsystem.dto.ScreeningDetailDTO;
import com.cinema.ticketsystem.service.ScreeningService;
import jakarta.validation.Valid;
import lombok.Getter;
import lombok.Setter;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/screenings")
@CrossOrigin(origins = "http://localhost:4200")
public class ScreeningController {
    
    @Autowired
    private ScreeningService screeningService;
    
    @GetMapping
    public ResponseEntity<List<ScreeningDTO>> getAllScreenings() {
        try {
            List<ScreeningDTO> screenings = screeningService.getAllScreenings();
            return ResponseEntity.ok(screenings);
        } catch (Exception e) {
            return ResponseEntity.badRequest().build();
        }
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<?> getScreening(@PathVariable Long id) {
        try {
            ScreeningDetailDTO screening = screeningService.getScreeningWithSeats(id);
            return ResponseEntity.ok(screening);
        } catch (Exception e) {
            return ResponseEntity.badRequest().body(new ErrorResponse(e.getMessage()));
        }
    }
    
    @PostMapping
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<?> createScreening(@Valid @RequestBody CreateScreeningRequest request) {
        try {
            ScreeningDTO screening = screeningService.createScreening(request);
            return ResponseEntity.ok(screening);
        } catch (Exception e) {
            return ResponseEntity.badRequest().body(new ErrorResponse(e.getMessage()));
        }
    }
    
    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<?> deleteScreening(@PathVariable Long id) {
        try {
            screeningService.deleteScreening(id);
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
