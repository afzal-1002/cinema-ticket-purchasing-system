package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.ScreeningDetailDTO;
import com.cinema.ticketsystem.dto.SeatDTO;
import com.cinema.ticketsystem.model.Screening;
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
        dto.setCinemaId(screening.getCinema() != null ? screening.getCinema().getId() : null);
        dto.setCinemaName(screening.getCinema() != null ? screening.getCinema().getName() : null);
        dto.setMovieId(screening.getMovie() != null ? screening.getMovie().getId() : null);
        dto.setMovieTitle(screening.getMovie() != null ? screening.getMovie().getTitle() : null);
        dto.setStartDateTime(screening.getStartDateTime());
        dto.setTicketPrice(screening.getTicketPrice());
        dto.setRows(screening.getCinema() != null ? screening.getCinema().getRows() : null);
        dto.setSeatsPerRow(screening.getCinema() != null ? screening.getCinema().getSeatsPerRow() : null);
        dto.setSeats(seats);
        return dto;
    }
}
