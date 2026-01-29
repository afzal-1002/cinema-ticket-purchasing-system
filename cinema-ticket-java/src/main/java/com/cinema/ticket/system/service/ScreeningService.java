package com.cinema.ticket.system.service;

import com.cinema.ticket.system.dto.CreateScreeningRequest;
import com.cinema.ticket.system.dto.ScreeningDTO;
import com.cinema.ticket.system.dto.ScreeningDetailDTO;

import java.util.List;

public interface ScreeningService {
    
    List<ScreeningDTO> getAllScreenings();
    
    ScreeningDetailDTO getScreeningWithSeats(Long id);
    
    ScreeningDTO createScreening(CreateScreeningRequest request);
    
    void deleteScreening(Long id);
}
