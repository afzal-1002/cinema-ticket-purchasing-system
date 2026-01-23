package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.CinemaDTO;
import com.cinema.ticketsystem.model.Cinema;
import org.springframework.stereotype.Component;

@Component
public class CinemaMapper {
    
    public CinemaDTO toDTO(Cinema cinema) {
        if (cinema == null) {
            return null;
        }
        
        CinemaDTO dto = new CinemaDTO();
        dto.setId(cinema.getId());
        dto.setName(cinema.getName());
        dto.setRows(cinema.getRows());
        dto.setSeatsPerRow(cinema.getSeatsPerRow());
        return dto;
    }
}
