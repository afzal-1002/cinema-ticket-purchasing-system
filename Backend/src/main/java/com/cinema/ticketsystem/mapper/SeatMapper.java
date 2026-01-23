package com.cinema.ticketsystem.mapper;

import com.cinema.ticketsystem.dto.SeatDTO;
import org.springframework.stereotype.Component;

@Component
public class SeatMapper {
    
    /**
     * Creates a SeatDTO for seat information
     * Note: Seat is not a persistent entity, so this is a utility mapper
     */
    public SeatDTO toDTO(Integer row, Integer seat, Boolean isReserved, Long userId) {
        if (row == null || seat == null) {
            return null;
        }
        
        SeatDTO dto = new SeatDTO();
        dto.setRow(row);
        dto.setSeat(seat);
        dto.setIsReserved(isReserved != null ? isReserved : false);
        dto.setUserId(userId);
        return dto;
    }
    
    /**
     * Creates a SeatDTO with default values
     */
    public SeatDTO toDTO(Integer row, Integer seat) {
        return toDTO(row, seat, false, null);
    }
}
