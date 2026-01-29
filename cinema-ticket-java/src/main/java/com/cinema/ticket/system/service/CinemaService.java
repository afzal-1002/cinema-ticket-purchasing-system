package com.cinema.ticket.system.service;

import com.cinema.ticket.system.dto.CinemaDTO;
import com.cinema.ticket.system.model.Cinema;

import java.util.List;

public interface CinemaService {
    
    List<CinemaDTO> getAllCinemas();
    
    Cinema getCinemaById(Long id);

    CinemaDTO createCinema(CinemaDTO cinemaDTO);

    CinemaDTO updateCinema(Long id, CinemaDTO cinemaDTO);

    void deleteCinema(Long id);
}
