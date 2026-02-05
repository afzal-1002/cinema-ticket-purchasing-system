package com.cinema.ticketsystem.controller;

import com.cinema.ticketsystem.security.UserDetailsImpl;
import com.cinema.ticketsystem.service.SeatSelectionService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/seats")
@RequiredArgsConstructor
public class SeatSelectionController {
    
    private final SeatSelectionService seatSelectionService;
    
    @GetMapping("/screening/{screeningId}")
    public ResponseEntity<Map<String, Object>> getAvailableSeats(@PathVariable Long screeningId) {
        return ResponseEntity.ok(seatSelectionService.getAvailableSeats(screeningId));
    }
    
    @PostMapping("/hold")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<Map<String, Object>> holdSeats(
            @AuthenticationPrincipal UserDetailsImpl userDetails,
            @RequestBody Map<String, Object> request) {
        Long screeningId = Long.valueOf(request.get("screeningId").toString());
        @SuppressWarnings("unchecked")
        List<Map<String, Integer>> seats = (List<Map<String, Integer>>) request.get("seats");
        
        return ResponseEntity.ok(seatSelectionService.holdSeats(userDetails.getId(), screeningId, seats));
    }
    
    @DeleteMapping("/hold")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<Void> releaseHolds(
            @AuthenticationPrincipal UserDetailsImpl userDetails,
            @RequestBody Map<String, List<Long>> request) {
        List<Long> holdIds = request.get("holdIds");
        seatSelectionService.releaseHolds(userDetails.getId(), holdIds);
        return ResponseEntity.noContent().build();
    }
    
    @DeleteMapping("/hold/all")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<Void> releaseAllHolds(@AuthenticationPrincipal UserDetailsImpl userDetails) {
        seatSelectionService.releaseAllUserHolds(userDetails.getId());
        return ResponseEntity.noContent().build();
    }
    
    @GetMapping("/hold/my-holds")
    @PreAuthorize("isAuthenticated()")
    public ResponseEntity<List<Map<String, Object>>> getMyHolds(
            @AuthenticationPrincipal UserDetailsImpl userDetails) {
        return ResponseEntity.ok(seatSelectionService.getUserActiveHolds(userDetails.getId()));
    }
}
