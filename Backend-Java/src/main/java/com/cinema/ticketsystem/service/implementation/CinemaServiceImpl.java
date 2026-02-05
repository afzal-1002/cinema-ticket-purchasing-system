package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.dto.CinemaDTO;
import com.cinema.ticketsystem.mapper.CinemaMapper;
import com.cinema.ticketsystem.model.Cinema;
import com.cinema.ticketsystem.repository.CinemaRepository;
import com.cinema.ticketsystem.service.CinemaService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class CinemaServiceImpl implements CinemaService {
    
    private final CinemaRepository cinemaRepository;
    private final CinemaMapper cinemaMapper;
    
    public List<CinemaDTO> getAllCinemas() {
        return cinemaRepository.findAll().stream()
                .map(cinemaMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    public Cinema getCinemaById(Long id) {
        if (id==null) { return    null;}
        return cinemaRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Cinema not found"));
    }
}
