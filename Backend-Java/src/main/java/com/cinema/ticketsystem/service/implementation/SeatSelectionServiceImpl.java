package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.model.*;
import com.cinema.ticketsystem.repository.*;
import com.cinema.ticketsystem.service.SeatSelectionService;
import lombok.RequiredArgsConstructor;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class SeatSelectionServiceImpl implements SeatSelectionService {
    
    private final SeatHoldRepository seatHoldRepository;
    private final ReservationRepository reservationRepository;
    private final ScreeningRepository screeningRepository;
    private final UserRepository userRepository;
    
    private static final int HOLD_DURATION_MINUTES = 15;
    
    @Transactional(readOnly = true)
    public Map<String, Object> getAvailableSeats(Long screeningId) {
        if (screeningId == null) {
            throw new RuntimeException("Screening ID cannot be null");
        }
        Screening screening = screeningRepository.findById(screeningId)
                .orElseThrow(() -> new RuntimeException("Screening not found"));
        
        Cinema cinema = screening.getCinema();
        int rows = cinema.getRows();
        int seatsPerRow = cinema.getSeatsPerRow();
        
        // Get all reservations for this screening
        List<Reservation> reservations = reservationRepository.findByScreeningId(screeningId);
        Set<String> reservedSeats = reservations.stream()
                .map(r -> r.getRow() + "-" + r.getSeat())
                .collect(Collectors.toSet());
        
        // Get all active seat holds
        List<SeatHold> activeHolds = seatHoldRepository.findByScreeningIdAndIsActiveTrue(screeningId);
        Set<String> heldSeats = activeHolds.stream()
                .filter(h -> h.getExpiresAt().isAfter(LocalDateTime.now()))
                .map(h -> h.getRow() + "-" + h.getSeat())
                .collect(Collectors.toSet());
        
        // Build seat map
        List<Map<String, Object>> seatMap = new ArrayList<>();
        for (int row = 1; row <= rows; row++) {
            for (int seat = 1; seat <= seatsPerRow; seat++) {
                String seatKey = row + "-" + seat;
                Map<String, Object> seatInfo = new HashMap<>();
                seatInfo.put("row", row);
                seatInfo.put("seat", seat);
                seatInfo.put("seatNumber", String.format("%c%d", (char)('A' + row - 1), seat));
                
                if (reservedSeats.contains(seatKey)) {
                    seatInfo.put("status", "RESERVED");
                } else if (heldSeats.contains(seatKey)) {
                    seatInfo.put("status", "HELD");
                } else {
                    seatInfo.put("status", "AVAILABLE");
                }
                
                seatMap.add(seatInfo);
            }
        }
        
        Map<String, Object> result = new HashMap<>();
        result.put("screeningId", screeningId);
        result.put("movieTitle", screening.getMovie().getTitle());
        result.put("startDateTime", screening.getStartDateTime());
        result.put("cinemaName", cinema.getName());
        result.put("totalRows", rows);
        result.put("seatsPerRow", seatsPerRow);
        result.put("totalSeats", rows * seatsPerRow);
        result.put("availableSeats", seatMap.stream().filter(s -> "AVAILABLE".equals(s.get("status"))).count());
        result.put("seats", seatMap);
        
        return result;
    }
    
    @Transactional
    public Map<String, Object> holdSeats(Long userId, Long screeningId, List<Map<String, Integer>> seats) {
        if (userId == null) {
            throw new RuntimeException("User ID cannot be null");
        }
        if (screeningId == null) {
            throw new RuntimeException("Screening ID cannot be null");
        }
        
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("User not found"));
        
        Screening screening = screeningRepository.findById(screeningId)
                .orElseThrow(() -> new RuntimeException("Screening not found"));
        
        // Release any expired holds first
        cleanupExpiredHolds();
        
        List<SeatHold> createdHolds = new ArrayList<>();
        LocalDateTime expiresAt = LocalDateTime.now().plusMinutes(HOLD_DURATION_MINUTES);
        
        for (Map<String, Integer> seat : seats) {
            Integer row = seat.get("row");
            Integer seatNumber = seat.get("seat");
            
            // Validate seat exists
            if (row < 1 || row > screening.getCinema().getRows() ||
                seatNumber < 1 || seatNumber > screening.getCinema().getSeatsPerRow()) {
                throw new RuntimeException("Invalid seat: Row " + row + ", Seat " + seatNumber);
            }
            
            // Check if seat is already reserved
            if (reservationRepository.existsByScreeningIdAndRowAndSeat(screeningId, row, seatNumber)) {
                throw new RuntimeException("Seat already reserved: Row " + row + ", Seat " + seatNumber);
            }
            
            // Check if seat is already held
            if (seatHoldRepository.existsByScreeningIdAndRowAndSeatAndIsActiveTrue(screeningId, row, seatNumber)) {
                throw new RuntimeException("Seat already held: Row " + row + ", Seat " + seatNumber);
            }
            
            // Create hold
            SeatHold hold = new SeatHold();
            hold.setUser(user);
            hold.setScreening(screening);
            hold.setRow(row);
            hold.setSeat(seatNumber);
            hold.setExpiresAt(expiresAt);
            hold.setIsActive(true);
            
            createdHolds.add(seatHoldRepository.save(hold));
        }
        
        Map<String, Object> result = new HashMap<>();
        result.put("holdIds", createdHolds.stream().map(SeatHold::getId).collect(Collectors.toList()));
        result.put("expiresAt", expiresAt);
        result.put("expiresInMinutes", HOLD_DURATION_MINUTES);
        result.put("seats", seats);
        
        return result;
    }
    
    @Transactional
    public void releaseHolds(Long userId, List<Long> holdIds) {
        for (Long holdId : holdIds) {
            if (holdId == null) {
                throw new RuntimeException("Hold ID cannot be null");
            }
            SeatHold hold = seatHoldRepository.findById(holdId)
                    .orElseThrow(() -> new RuntimeException("Hold not found"));
            
            if (!hold.getUser().getId().equals(userId)) {
                throw new RuntimeException("You can only release your own holds");
            }
            
            hold.setIsActive(false);
            seatHoldRepository.save(hold);
        }
    }
    
    @Transactional
    public void releaseAllUserHolds(Long userId) {
        List<SeatHold> userHolds = seatHoldRepository.findByUserIdAndIsActiveTrue(userId);
        userHolds.forEach(hold -> hold.setIsActive(false));
        seatHoldRepository.saveAll(userHolds);
    }
    
    @Transactional(readOnly = true)
    public List<Map<String, Object>> getUserActiveHolds(Long userId) {
        List<SeatHold> holds = seatHoldRepository.findByUserIdAndIsActiveTrue(userId);
        
        return holds.stream()
                .filter(h -> h.getExpiresAt().isAfter(LocalDateTime.now()))
                .map(h -> {
                    Map<String, Object> holdInfo = new HashMap<>();
                    holdInfo.put("holdId", h.getId());
                    holdInfo.put("screeningId", h.getScreening().getId());
                    holdInfo.put("movieTitle", h.getScreening().getMovie().getTitle());
                    holdInfo.put("row", h.getRow());
                    holdInfo.put("seat", h.getSeat());
                    holdInfo.put("seatNumber", String.format("%c%d", (char)('A' + h.getRow() - 1), h.getSeat()));
                    holdInfo.put("heldAt", h.getHeldAt());
                    holdInfo.put("expiresAt", h.getExpiresAt());
                    return holdInfo;
                })
                .collect(Collectors.toList());
    }
    
    @Transactional
    @Scheduled(fixedRate = 60000) // Run every minute
    public void cleanupExpiredHolds() {
        int deactivated = seatHoldRepository.deactivateExpiredHolds(LocalDateTime.now());
        if (deactivated > 0) {
            System.out.println("Deactivated " + deactivated + " expired seat holds");
        }
    }
}
