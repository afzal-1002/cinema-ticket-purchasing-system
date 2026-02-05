package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.dto.CreateScreeningRequest;
import com.cinema.ticketsystem.dto.ScreeningDTO;
import com.cinema.ticketsystem.dto.ScreeningDetailDTO;

import java.util.List;

public interface ScreeningService {
    
    List<ScreeningDTO> getAllScreenings();
    
    ScreeningDetailDTO getScreeningWithSeats(Long id);
    
    ScreeningDTO createScreening(CreateScreeningRequest request);
    
    void deleteScreening(Long id);
}
