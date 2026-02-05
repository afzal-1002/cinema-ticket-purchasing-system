package com.cinema.ticketsystem.controller;

import com.cinema.ticketsystem.service.AnalyticsService;
import lombok.RequiredArgsConstructor;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDate;
import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/analytics")
@RequiredArgsConstructor
@PreAuthorize("hasRole('ADMIN')")
public class AnalyticsController {
    
    private final AnalyticsService analyticsService;
    
    @GetMapping("/dashboard")
    public ResponseEntity<Map<String, Object>> getDashboardOverview() {
        return ResponseEntity.ok(analyticsService.getDashboardOverview());
    }
    
    @GetMapping("/revenue")
    public ResponseEntity<Map<String, Object>> getRevenueAnalytics(
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate startDate,
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate endDate) {
        return ResponseEntity.ok(analyticsService.getRevenueAnalytics(startDate, endDate));
    }
    
    @GetMapping("/popular-movies")
    public ResponseEntity<List<Map<String, Object>>> getPopularMovies(
            @RequestParam(defaultValue = "10") int limit) {
        return ResponseEntity.ok(analyticsService.getPopularMovies(limit));
    }
    
    @GetMapping("/occupancy/screening/{screeningId}")
    public ResponseEntity<Map<String, Object>> getScreeningOccupancy(@PathVariable Long screeningId) {
        return ResponseEntity.ok(analyticsService.getOccupancyStatistics(screeningId));
    }
    
    @GetMapping("/occupancy/movies")
    public ResponseEntity<List<Map<String, Object>>> getMovieOccupancy(
            @RequestParam(defaultValue = "10") int limit) {
        return ResponseEntity.ok(analyticsService.getAverageOccupancyByMovie(limit));
    }
    
    @GetMapping("/booking-trends")
    public ResponseEntity<Map<String, Object>> getBookingTrends(
            @RequestParam(defaultValue = "30") int days) {
        return ResponseEntity.ok(analyticsService.getBookingTrends(days));
    }
}
