package com.cinema.ticket.system.mapper;

import com.cinema.ticket.system.dto.SeatHoldDTO;
import com.cinema.ticket.system.model.SeatHold;
import org.springframework.stereotype.Component;

@Component
public class SeatHoldMapper {
    
    public SeatHoldDTO toDTO(SeatHold seatHold) {
        if (seatHold == null) {
            return null;
        }
        
        SeatHoldDTO dto = new SeatHoldDTO();
        dto.setId(seatHold.getId());
        dto.setUserId(seatHold.getUser() != null ? seatHold.getUser().getId() : null);
        dto.setScreeningId(seatHold.getScreening() != null ? seatHold.getScreening().getId() : null);
        dto.setRow(seatHold.getRow());
        dto.setSeat(seatHold.getSeat());
        dto.setHeldAt(seatHold.getHeldAt());
        dto.setExpiresAt(seatHold.getExpiresAt());
        dto.setIsActive(seatHold.getIsActive());
        return dto;
    }
}
