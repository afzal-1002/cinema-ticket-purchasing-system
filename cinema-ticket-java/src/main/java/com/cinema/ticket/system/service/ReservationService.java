package com.cinema.ticket.system.service;

import com.cinema.ticket.system.dto.CreateReservationRequest;
import com.cinema.ticket.system.dto.ReservationDTO;

import java.util.List;

public interface ReservationService {
    
    ReservationDTO createReservation(Long userId, CreateReservationRequest request);
    
    void deleteReservation(Long userId, Long screeningId, Integer row, Integer seat);
    
    List<ReservationDTO> getUserReservations(Long userId);
}
