package com.cinema.ticketsystem.controller;

import com.cinema.ticketsystem.dto.CinemaDTO;
import com.cinema.ticketsystem.service.CinemaService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
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
}
