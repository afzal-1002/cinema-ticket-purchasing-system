package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.dto.CreateReservationRequest;
import com.cinema.ticketsystem.dto.ReservationDTO;
import com.cinema.ticketsystem.mapper.ReservationMapper;
import com.cinema.ticketsystem.model.Reservation;
import com.cinema.ticketsystem.model.Screening;
import com.cinema.ticketsystem.model.User;
import com.cinema.ticketsystem.repository.ReservationRepository;
import com.cinema.ticketsystem.repository.ScreeningRepository;
import com.cinema.ticketsystem.service.ReservationService;
import com.cinema.ticketsystem.service.UserService;
import jakarta.persistence.OptimisticLockException;
import lombok.RequiredArgsConstructor;
import org.springframework.dao.DataIntegrityViolationException;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class ReservationServiceImpl implements ReservationService {
    
    private final ReservationRepository reservationRepository;
    private final ScreeningRepository screeningRepository;
    private final UserService userService;
    private final ReservationMapper reservationMapper;
    
    @Transactional
    public ReservationDTO createReservation(Long userId, CreateReservationRequest request) {
        Long screeningId = request.getScreeningId();
        if (screeningId == null) {
            throw new IllegalArgumentException("Screening ID cannot be null");
        }
        Screening screening = screeningRepository.findById(screeningId)
                .orElseThrow(() -> new RuntimeException("Screening not found"));
        
        User user = userService.getUserEntityById(userId);
        
        // Validate seat position
        if (request.getRow() < 1 || request.getRow() > screening.getCinema().getRows() ||
            request.getSeat() < 1 || request.getSeat() > screening.getCinema().getSeatsPerRow()) {
            throw new IllegalArgumentException("Invalid seat position");
        }
        
        // Check if seat is already reserved (handled by unique constraint, but checking here for better error message)
        if (reservationRepository.existsByScreeningIdAndRowAndSeat(
                request.getScreeningId(), request.getRow(), request.getSeat())) {
            throw new RuntimeException("This seat is already reserved. Please select another seat.");
        }
        
        Reservation reservation = new Reservation();
        reservation.setUser(user);
        reservation.setScreening(screening);
        reservation.setRow(request.getRow());
        reservation.setSeat(request.getSeat());
        
        try {
            Reservation savedReservation = reservationRepository.save(reservation);
            return reservationMapper.toDTO(savedReservation);
        } catch (DataIntegrityViolationException e) {
            // This handles concurrent reservations of the same seat
            throw new RuntimeException("This seat is already reserved. Please select another seat.");
        } catch (OptimisticLockException e) {
            throw new RuntimeException("This seat was just reserved by another user. Please select another seat.");
        }
    }
    
    @Transactional
    public void deleteReservation(Long userId, Long screeningId, Integer row, Integer seat) {
        Reservation reservation = reservationRepository
                .findByUserIdAndScreeningIdAndRowAndSeat(userId, screeningId, row, seat)
                .orElseThrow(() -> new RuntimeException("Reservation not found"));
        
        if (reservation != null) {
            reservationRepository.delete(reservation);
        }
    }
    
    public List<ReservationDTO> getUserReservations(Long userId) {
        return reservationRepository.findByUserIdOrderByScreening_StartDateTimeAsc(userId).stream()
                .map(reservationMapper::toDTO)
                .collect(Collectors.toList());
    }
}
