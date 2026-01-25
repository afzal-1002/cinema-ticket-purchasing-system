package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.dto.CinemaDTO;
import com.cinema.ticketsystem.mapper.CinemaMapper;
import com.cinema.ticketsystem.model.Cinema;
import com.cinema.ticketsystem.repository.CinemaRepository;
import com.cinema.ticketsystem.service.CinemaService;
import jakarta.persistence.EntityNotFoundException;
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
        if (id == null || id <= 0) {
            throw new IllegalArgumentException("Invalid cinema ID");
        }
        return cinemaRepository.findById(id)
                .orElseThrow(() -> new EntityNotFoundException("Cinema not found"));
    }

    @SuppressWarnings("null") // Generic exception used for simplicity
    @Override
    public CinemaDTO createCinema(CinemaDTO cinemaDTO) {
        if (cinemaDTO == null) {
            throw new IllegalArgumentException("Cinema payload is required");
        }
        Cinema cinema = cinemaMapper.toEntity(cinemaDTO);
        Cinema saved = cinemaRepository.save(cinema);
        return cinemaMapper.toDTO(saved);
    }

    @SuppressWarnings("null") // Generic exception used for simplicity
    @Override
    public CinemaDTO updateCinema(Long id, CinemaDTO cinemaDTO) {
        if (id == null || id <= 0) {
            throw new IllegalArgumentException("Invalid cinema ID");
        }
        if (cinemaDTO == null) {
            throw new IllegalArgumentException("Cinema payload is required");
        }

        Cinema cinema = cinemaRepository.findById(id)
                .orElseThrow(() -> new EntityNotFoundException("Cinema not found"));
        cinemaMapper.updateEntity(cinema, cinemaDTO);
        Cinema saved = cinemaRepository.save(cinema);
        return cinemaMapper.toDTO(saved);
    }

    @SuppressWarnings("null") // Generic exception used for simplicity
    @Override
    public void deleteCinema(Long id) {
        if (id == null || id <= 0) {
            throw new IllegalArgumentException("Invalid cinema ID");
        }
        Cinema cinema = cinemaRepository.findById(id)
                .orElseThrow(() -> new EntityNotFoundException("Cinema not found"));
        cinemaRepository.delete(cinema);
    }
}
