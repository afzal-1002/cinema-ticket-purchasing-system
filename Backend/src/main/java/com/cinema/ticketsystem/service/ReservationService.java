package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.dto.CreateReservationRequest;
import com.cinema.ticketsystem.dto.ReservationDTO;

import java.util.List;

public interface ReservationService {
    
    ReservationDTO createReservation(Long userId, CreateReservationRequest request);
    
    void deleteReservation(Long userId, Long screeningId, Integer row, Integer seat);
    
    List<ReservationDTO> getUserReservations(Long userId);
}
