package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.ScreeningDTO;
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
        dto.setCinemaId(screening.getCinema().getId());
        dto.setCinemaName(screening.getCinema().getName());
        dto.setMovieId(screening.getMovie().getId());
        dto.setMovieTitle(screening.getMovie().getTitle());
        dto.setStartDateTime(screening.getStartDateTime());
        dto.setTicketPrice(screening.getTicketPrice());
        dto.setRows(screening.getCinema().getRows());
        dto.setSeatsPerRow(screening.getCinema().getSeatsPerRow());
        return dto;
    }
}
