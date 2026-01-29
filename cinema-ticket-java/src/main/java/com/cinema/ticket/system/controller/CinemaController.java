package com.cinema.ticket.system.controller;

import com.cinema.ticket.system.dto.CinemaDTO;
import com.cinema.ticket.system.service.CinemaService;
import jakarta.persistence.EntityNotFoundException;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/cinemas")
@CrossOrigin(origins = "http://localhost:4200")
public class CinemaController {
    
    @Autowired
    private CinemaService cinemaService;
    
    @GetMapping
    public ResponseEntity<List<CinemaDTO>> getAllCinemas() {
        try {
            List<CinemaDTO> cinemas = cinemaService.getAllCinemas();
            return ResponseEntity.ok(cinemas);
        } catch (Exception e) {
            return ResponseEntity.badRequest().build();
        }
    }

    @PostMapping
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<CinemaDTO> createCinema(@Valid @RequestBody CinemaDTO cinemaDTO) {
        try {
            CinemaDTO created = cinemaService.createCinema(cinemaDTO);
            return ResponseEntity.status(HttpStatus.CREATED).body(created);
        } catch (IllegalArgumentException e) {
            return ResponseEntity.badRequest().build();
        }
    }

    @PutMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<CinemaDTO> updateCinema(@PathVariable Long id, @Valid @RequestBody CinemaDTO cinemaDTO) {
        try {
            CinemaDTO updated = cinemaService.updateCinema(id, cinemaDTO);
            return ResponseEntity.ok(updated);
        } catch (EntityNotFoundException e) {
            return ResponseEntity.notFound().build();
        } catch (IllegalArgumentException e) {
            return ResponseEntity.badRequest().build();
        }
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<Void> deleteCinema(@PathVariable Long id) {
        try {
            cinemaService.deleteCinema(id);
            return ResponseEntity.noContent().build();
        } catch (EntityNotFoundException e) {
            return ResponseEntity.notFound().build();
        } catch (IllegalArgumentException e) {
            return ResponseEntity.badRequest().build();
        }
    }
}
