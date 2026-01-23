package com.cinema.ticketsystem.service;

import com.cinema.ticketsystem.dto.CinemaDTO;
import com.cinema.ticketsystem.model.Cinema;

import java.util.List;

public interface CinemaService {
    
    List<CinemaDTO> getAllCinemas();
    
    Cinema getCinemaById(Long id);
}
