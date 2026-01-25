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

    public Cinema toEntity(CinemaDTO dto) {
        if (dto == null) {
            return null;
        }

        Cinema cinema = new Cinema();
        cinema.setName(dto.getName());
        cinema.setRows(dto.getRows());
        cinema.setSeatsPerRow(dto.getSeatsPerRow());
        return cinema;
    }

    public void updateEntity(Cinema cinema, CinemaDTO dto) {
        if (cinema == null || dto == null) {
            return;
        }
        cinema.setName(dto.getName());
        cinema.setRows(dto.getRows());
        cinema.setSeatsPerRow(dto.getSeatsPerRow());
    }
}
