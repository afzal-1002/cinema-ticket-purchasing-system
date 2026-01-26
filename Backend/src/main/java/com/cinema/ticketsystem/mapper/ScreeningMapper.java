package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.ScreeningDTO;
import com.cinema.ticketsystem.model.Cinema;
import com.cinema.ticketsystem.model.Screening;
import org.springframework.stereotype.Component;

@Component
public class ScreeningMapper {
    
    public ScreeningDTO toDTO(Screening screening) {
        if (screening == null) {
            return null;
        }
        
        ScreeningDTO dto = new ScreeningDTO();
        dto.setId(screening.getId());
        Cinema cinema = screening.getCinema();
        if (cinema != null) {
            dto.setCinemaId(cinema.getId());
            dto.setCinemaName(cinema.getName());
            dto.setRows(cinema.getRows());
            dto.setSeatsPerRow(cinema.getSeatsPerRow());
            applySeatMetrics(screening, cinema, dto);
        }
        dto.setMovieId(screening.getMovie().getId());
        dto.setMovieTitle(screening.getMovie().getTitle());
        dto.setStartDateTime(screening.getStartDateTime());
        dto.setTicketPrice(screening.getTicketPrice());
        return dto;
    }

    private void applySeatMetrics(Screening screening, Cinema cinema, ScreeningDTO dto) {
        int rows = cinema.getRows() != null ? cinema.getRows() : 0;
        int seatsPerRow = cinema.getSeatsPerRow() != null ? cinema.getSeatsPerRow() : 0;
        int totalSeats = rows * seatsPerRow;
        int reservedSeats = screening.getReservations() != null ? screening.getReservations().size() : 0;
        long activeHolds = screening.getSeatHolds() == null ? 0L
                : screening.getSeatHolds().stream()
                .filter(hold -> Boolean.TRUE.equals(hold.getIsActive()))
                .count();
        int heldSeats = Math.toIntExact(activeHolds);
        int availableSeats = Math.max(totalSeats - reservedSeats - heldSeats, 0);

        dto.setTotalSeats(totalSeats);
        dto.setReservedSeats(reservedSeats);
        dto.setHeldSeats(heldSeats);
        dto.setAvailableSeats(availableSeats);
    }
}
