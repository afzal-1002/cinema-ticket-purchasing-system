package com.cinema.ticketsystem.service;

import java.util.List;
import java.util.Map;

public interface SeatSelectionService {
    
    Map<String, Object> getAvailableSeats(Long screeningId);
    
    Map<String, Object> holdSeats(Long userId, Long screeningId, List<Map<String, Integer>> seats);
    
    void releaseHolds(Long userId, List<Long> holdIds);
    
    void releaseAllUserHolds(Long userId);
    
    List<Map<String, Object>> getUserActiveHolds(Long userId);
    
    void cleanupExpiredHolds();
}
