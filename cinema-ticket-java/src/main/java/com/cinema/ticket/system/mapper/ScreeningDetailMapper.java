package com.cinema.ticket.system.mapper;

import com.cinema.ticket.system.dto.ScreeningDetailDTO;
import com.cinema.ticket.system.dto.SeatDTO;
import com.cinema.ticket.system.model.Cinema;
import com.cinema.ticket.system.model.Screening;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
@RequiredArgsConstructor
public class ScreeningDetailMapper {
    
    public ScreeningDetailDTO toDTO(Screening screening, List<SeatDTO> seats) {
        if (screening == null) {
            return null;
        }
        
        ScreeningDetailDTO dto = new ScreeningDetailDTO();
        dto.setId(screening.getId());
        Cinema cinema = screening.getCinema();
        dto.setCinemaId(cinema != null ? cinema.getId() : null);
        dto.setCinemaName(cinema != null ? cinema.getName() : null);
        dto.setMovieId(screening.getMovie() != null ? screening.getMovie().getId() : null);
        dto.setMovieTitle(screening.getMovie() != null ? screening.getMovie().getTitle() : null);
        dto.setStartDateTime(screening.getStartDateTime());
        dto.setTicketPrice(screening.getTicketPrice());
        dto.setRows(cinema != null ? cinema.getRows() : null);
        dto.setSeatsPerRow(cinema != null ? cinema.getSeatsPerRow() : null);
        applySeatMetrics(screening, cinema, dto);
        dto.setSeats(seats);
        return dto;
    }

    private void applySeatMetrics(Screening screening, Cinema cinema, ScreeningDetailDTO dto) {
        if (cinema == null) {
            dto.setTotalSeats(null);
            dto.setReservedSeats(null);
            dto.setHeldSeats(null);
            dto.setAvailableSeats(null);
            return;
        }
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
