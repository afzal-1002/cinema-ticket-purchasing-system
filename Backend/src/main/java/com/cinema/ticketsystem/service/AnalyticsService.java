package com.cinema.ticketsystem.service;

import java.time.LocalDate;
import java.util.List;
import java.util.Map;

public interface AnalyticsService {
    
    Map<String, Object> getDashboardOverview();
    
    Map<String, Object> getRevenueAnalytics(LocalDate startDate, LocalDate endDate);
    
    List<Map<String, Object>> getPopularMovies(int limit);
    
    Map<String, Object> getOccupancyStatistics(Long screeningId);
    
    List<Map<String, Object>> getAverageOccupancyByMovie(int limit);
    
    Map<String, Object> getBookingTrends(int days);
}
