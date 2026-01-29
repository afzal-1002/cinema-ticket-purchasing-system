package com.cinema.ticket.system.mapper;

import com.cinema.ticket.system.dto.ReservationDTO;
import com.cinema.ticket.system.model.Reservation;
import org.springframework.stereotype.Component;

@Component
public class ReservationMapper {
    
    public ReservationDTO toDTO(Reservation reservation) {
        if (reservation == null) {
            return null;
        }
        
        ReservationDTO dto = new ReservationDTO();
        dto.setId(reservation.getId());
        dto.setScreeningId(reservation.getScreening().getId());
        dto.setMovieTitle(reservation.getScreening().getMovie().getTitle());
        dto.setStartDateTime(reservation.getScreening().getStartDateTime());
        dto.setCinemaName(reservation.getScreening().getCinema().getName());
        dto.setRow(reservation.getRow());
        dto.setSeat(reservation.getSeat());
        dto.setCreatedAt(reservation.getCreatedAt());
        return dto;
    }
}
